using System;
using System.Threading.Tasks;

namespace Core.Cache
{
    public class CachedQueryBuilder<T>
    {
        IQuery<T> query_;
        bool enableCache_ = true;

        public CachedQueryBuilder<T> SetCached(bool enableCache)
        {
            enableCache_ = enableCache;
            return this;
        }

        public CachedQueryBuilder<T> Query(IQuery<T> query)
        {
            query_ = query;
            return this;
        }

        public async Task<T> ExecuteAsync(IQueryCache<T> cache)
        {
            if (enableCache_)
                return await cache.QueryAsync(query_);
            else
                return await cache.QueryIgnoreCacheAsync(query_);
        }
    };

}
