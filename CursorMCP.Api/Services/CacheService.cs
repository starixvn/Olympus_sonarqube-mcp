using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CursorMCP.Api.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly bool _isEnabled;

        public CacheService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _isEnabled = configuration.GetValue<bool>("Caching:Enabled");
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (!_isEnabled)
            {
                return Task.FromResult<T?>(default);
            }

            return Task.FromResult(_cache.TryGetValue(key, out T? value) ? value : default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (!_isEnabled)
            {
                return Task.CompletedTask;
            }

            var expirationTime = expiration ?? TimeSpan.FromMinutes(_configuration.GetValue<int>("Caching:DefaultExpirationMinutes"));
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expirationTime);

            _cache.Set(key, value, cacheEntryOptions);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_isEnabled)
            {
                return await factory();
            }

            if (_cache.TryGetValue(key, out T? cachedValue))
            {
                return cachedValue ?? await factory();
            }

            var value = await factory();
            var expirationTime = expiration ?? TimeSpan.FromMinutes(_configuration.GetValue<int>("Caching:DefaultExpirationMinutes"));
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expirationTime);

            _cache.Set(key, value, cacheEntryOptions);
            
            return value;
        }

        public void Remove(string key)
        {
            if (_isEnabled)
            {
                _cache.Remove(key);
            }
        }
    }
} 