namespace CursorMCP.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

public class CacheService(IMemoryCache cache, IConfiguration configuration) : ICacheService
{
    private readonly IMemoryCache cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IConfiguration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly bool isEnabled = configuration.GetValue<bool>("Caching:Enabled");

    public Task<T?> GetAsync<T>(string key)
    {
        return !isEnabled 
            ? Task.FromResult<T?>(default) 
            : Task.FromResult(cache.TryGetValue(key, out T? value) ? value : default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (!isEnabled)
        {
            return Task.CompletedTask;
        }

        var expirationTime = expiration ?? TimeSpan.FromMinutes(configuration.GetValue<int>("Caching:DefaultExpirationMinutes"));
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expirationTime);

        cache.Set(key, value, cacheEntryOptions);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (!isEnabled)
        {
            return await factory();
        }

        if (cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue ?? await factory();
        }

        var value = await factory();
        var expirationTime = expiration ?? TimeSpan.FromMinutes(configuration.GetValue<int>("Caching:DefaultExpirationMinutes"));
            
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expirationTime);

        cache.Set(key, value, cacheEntryOptions);
            
        return value;
    }

    public void Remove(string key)
    {
        if (isEnabled)
        {
            cache.Remove(key);
        }
    }
}