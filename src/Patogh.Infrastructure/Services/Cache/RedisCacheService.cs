using System.Text.Json;
using Patogh.Application.Interfaces;
using StackExchange.Redis;

namespace Patogh.Infrastructure.Services.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    // JsonSerializerOptions are created once and reused — options objects are expensive
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<T>(value!, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var serialized = JsonSerializer.Serialize(value, JsonOptions);
        await _db.StringSetAsync(key, serialized, expiry ?? TimeSpan.FromMinutes(5));
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // KEYS command should only be used in development/low-traffic scenarios.
        // For production with millions of keys, use SCAN instead.
        // This implementation is sufficient for MVP scale.
        var server = _db.Multiplexer.GetServer(
            _db.Multiplexer.GetEndPoints().First());

        var keys = server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0)
            await _db.KeyDeleteAsync(keys);
    }
}