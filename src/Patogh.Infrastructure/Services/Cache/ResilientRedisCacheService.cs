using System.Text.Json;
using Microsoft.Extensions.Logging;
using Patogh.Application.Interfaces;
using StackExchange.Redis;

namespace Patogh.Infrastructure.Services.Cache;

/// <summary>
/// Redis cache service with graceful fallback to a no-op when Redis is
/// unavailable. Unlike the bare <see cref="RedisCacheService"/>, every Redis
/// operation is wrapped in a try/catch so a transient Redis outage does not
/// propagate as a 500 to API callers — the app just runs without caching.
/// </summary>
public class ResilientRedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ResilientRedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ResilientRedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<ResilientRedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (!_redis.IsConnected)
                return null;

            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key '{Key}'. Returning cache miss.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            if (!_redis.IsConnected)
                return;

            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(value, JsonOptions);
            await db.StringSetAsync(key, serialized, expiry ?? TimeSpan.FromMinutes(5));
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key '{Key}'. Skipping cache write.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            if (!_redis.IsConnected)
                return;

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis DEL failed for key '{Key}'.", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            if (!_redis.IsConnected)
                return;

            var db = _redis.GetDatabase();
            // Use SCAN (not KEYS) for production safety — won't block Redis.
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.KeysAsync(pattern: pattern);

            await foreach (var key in keys)
            {
                try { await db.KeyDeleteAsync(key); }
                catch (RedisException) { /* best-effort */ }
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex,
                "Redis pattern delete failed for pattern '{Pattern}'.", pattern);
        }
    }
}
