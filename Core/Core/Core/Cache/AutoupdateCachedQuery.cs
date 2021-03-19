using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Cache
{
    class Entry<T>
    {
        public T Data;
        public IQuery<T> Query;
        public DateTime DataTime;
    }

    public class AutoUpdateCachedQuery<T> : IQueryCache<T>
    {
        public const int CACHE_TIME_OUT = 300;     // time to live in the cache
        public const int UPDATE_INTERVAL = 10;     // time to check for outdated entries
        public const int MAX_UPDATE_COUNT = 3;     // maximum number of query to perform in a single update step

        private ConcurrentDictionary<string, Entry<T>> cache_;
        private readonly IServiceProvider serviceProvider_;

        public AutoUpdateCachedQuery(
            IServiceProvider serviceProvider)
        {
            cache_ = new ConcurrentDictionary<string, Entry<T>>();
            serviceProvider_ = serviceProvider;
        }

        public async Task<T> QueryAsync(IQuery<T> query)
        {
            var key = query.Key;
            if (key == null)
                return await QueryIgnoreCacheAsync(query);

            Entry<T> cacheEntry;
            var now = DateTime.UtcNow;
            if (!cache_.TryGetValue(key, out cacheEntry) || (cacheEntry.DataTime + TimeSpan.FromSeconds(CACHE_TIME_OUT) < now))
            {
                // not in the cache or expired

                var data = await QueryIgnoreCacheAsync(query);

                cacheEntry = new Entry<T>
                {
                    Data = data,
                    Query = query,
                    DataTime = DateTime.UtcNow
                };

                // preserve the newst one (if someone had upadted it)
                cache_.AddOrUpdate(key, cacheEntry, (k, old) => old.DataTime >= cacheEntry.DataTime ? old : cacheEntry);
            }
            return cacheEntry.Data;
        }

        public async Task<T> QueryIgnoreCacheAsync(IQuery<T> query)
        {
            using (var scope = serviceProvider_.CreateScope())
            {
                return await query.Query(scope.ServiceProvider);
            }
        }

        internal async Task UpdateCache()
        {
            var values = cache_.Values; // creates a copy of the contained items. might be slow as during copy the all dict is locked.
            int count = 0;
            foreach (var v in values)
            {
                var now = DateTime.UtcNow;
                if (v.DataTime + TimeSpan.FromSeconds(CACHE_TIME_OUT) < now)
                {
                    // cache expired                                 
                    var data = await QueryIgnoreCacheAsync(v.Query);
                    count += 1;

                    // shall be ok, as this is the only place where an entry is mutated (at other places the object is replaced with a new one)
                    lock (v)
                    {

                        v.Data = data;
                        v.DataTime = now;

                    }
                }

                if (count > MAX_UPDATE_COUNT)
                    break;
            }
        }
    }

    public abstract class SafeHostedService : IHostedService, IDisposable
    {
        protected readonly ILogger logger_;
        private Timer timer_;
        private int executing_;

        public SafeHostedService(ILogger logger) => logger_ = logger;

        public void Dispose() => timer_?.Dispose();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            timer_ = new Timer(async _ => await OnTimer(), null, TimeSpan.Zero, Period);
            await Task.CompletedTask;
        }

        private async Task OnTimer()
        {
            if (Interlocked.CompareExchange(ref executing_, 1, 0) == 0) // avoid overlap of sync running
            {
                try
                {
                    await Run();
                }
                catch (AggregateException ex)
                {
                    logger_.LogCritical(ex.Flatten(), "Failed background service execution");
                }
                catch (Exception ex)
                {
                    logger_.LogCritical(ex, "Failed background service execution");
                }
                finally
                {
                    Volatile.Write(ref executing_, 0);
                }
            }
        }

        protected abstract Task Run();

        protected abstract TimeSpan Period { get; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer_?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }

    class CacheAutoUpdate<T> : SafeHostedService
    {
        private readonly IServiceProvider serviceProvider_;

        public CacheAutoUpdate(ILogger<CacheAutoUpdate<T>> logger, IServiceProvider serviceProvider) : base(logger)
        {
            serviceProvider_ = serviceProvider;
        }

        protected override TimeSpan Period => TimeSpan.FromSeconds(AutoUpdateCachedQuery<T>.UPDATE_INTERVAL);

        protected override async Task Run()
        {
            using (var scope = serviceProvider_.CreateScope())
            {
                var cache = serviceProvider_.GetRequiredService<AutoUpdateCachedQuery<T>>();
                await cache.UpdateCache();
            }
        }
    }

    public static class AutoUpdateCachedQueryExtensions
    {
        public static IServiceCollection AddAutoUpdateCachedQuery<T>(this IServiceCollection services)
        {
            services.AddHostedService<CacheAutoUpdate<T>>();
            services.TryAddSingleton<AutoUpdateCachedQuery<T>>();
            return services;
        }
    }


}
