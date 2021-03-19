using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Web
{
    public static class RateLimitingExtension
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            //todo: https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/Using-Redis-as-a-distributed-counter-store
            //services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            //services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            return services;
        }
    }
}
