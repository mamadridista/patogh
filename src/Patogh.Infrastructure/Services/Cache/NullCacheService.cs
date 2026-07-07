using Patogh.Application.Interfaces;

namespace Patogh.Infrastructure.Services.Cache;

// Used when Redis connection string is not configured.
// Returns null for all Gets (cache miss), silently ignores Sets and Removes.
// This lets the application run in environments without Redis (basic local dev).
public class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key) where T : class
        => Task.FromResult<T?>(null);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        => Task.CompletedTask;

    public Task RemoveAsync(string key)
        => Task.CompletedTask;

    public Task RemoveByPatternAsync(string pattern)
        => Task.CompletedTask;
}