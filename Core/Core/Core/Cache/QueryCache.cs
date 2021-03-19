using System;
using System.Threading.Tasks;

namespace Core.Cache
{
    // Helper to solce scope issue during cache. In a cached query we don't want to (can't) capture anything from the scope of the query creation.
    // There are timer tiggered cache policies and hence the query shall be executed without any local scope.

    public interface IQuery<T>
    {
        // The key for the cache, return null to disabled cache
        string Key { get; }

        Task<T> Query(IServiceProvider services);
    }

    public interface IQueryCache<T>
    {
        Task<T> QueryAsync(IQuery<T> query);

        Task<T> QueryIgnoreCacheAsync(IQuery<T> query);
    }
}
