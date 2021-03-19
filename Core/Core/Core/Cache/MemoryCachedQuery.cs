using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading.Tasks;

namespace Core.Cache
{
    public class MemoryCachedQuery<T> : IQueryCache<T>
    {
        private IMemoryCache cache_;
        private readonly IServiceProvider serviceProvider_;

        public MemoryCachedQuery(
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider)
        {
            cache_ = memoryCache;
            serviceProvider_ = serviceProvider;
        }

        public async Task<T> QueryAsync(IQuery<T> query)
        {
            var key = query.Key;
            if (key == null)
                return await QueryIgnoreCacheAsync(query);

            T cacheEntry;
            if (!cache_.TryGetValue(key, out cacheEntry))// Look for cache key.
            {
                // Key not in cache, so get data.
                cacheEntry = await QueryIgnoreCacheAsync(query);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1) //Size amount
                    .SetPriority(CacheItemPriority.High) //Priority on removing when reaching size limit (memory pressure)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30)) // Keep in cache for this time, reset time if accessed.
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)); // Remove from cache after this time, regardless of sliding expiration

                cache_.Set(key, cacheEntry, cacheEntryOptions);
            }
            return cacheEntry;
        }

        public async Task<T> QueryIgnoreCacheAsync(IQuery<T> query)
        {
            using (var scope = serviceProvider_.CreateScope())
            {
                return await query.Query(scope.ServiceProvider);
            }
        }
    }

    public static class MemoryCachedQueryExtensions
    {
        public static IServiceCollection AddMemoryCachedQuery<T>(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<MemoryCachedQuery<T>>();
            return services;
        }
    }

}
