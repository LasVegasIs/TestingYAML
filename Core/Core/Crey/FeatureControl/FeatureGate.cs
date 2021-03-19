using Core.Azure;
using Core.Crey.FeatureControl;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Exceptions;
using Crey.Kernel.ServiceDiscovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    public class CachedFeatureGate : IFeatureGate
    {
        private readonly ICreyService<FeatureGateStore> featureStore_;
        private readonly Dictionary<string, bool> features_;
        private readonly Dictionary<string, bool> chaos_;
        private readonly IServiceProvider services_;

        public CachedFeatureGate(ICreyService<FeatureGateStore> featureStore, IServiceProvider services)
        {
            featureStore_ = featureStore;
            features_ = new Dictionary<string, bool>();
            chaos_ = new Dictionary<string, bool>();
            services_ = services;
        }

        public async Task<Dictionary<string, bool>> GetFeaturesAsync(IEnumerable<string> names)
        {
            var result = new Dictionary<string, bool>();

            var toQuery = new List<string>();
            //collect all known
            lock (features_)
            {
                foreach (var name in names)
                {
                    if (features_.TryGetValue(name, out var value))
                        result[name] = value;
                    else
                        toQuery.Add(name);
                }
            }

            if (toQuery.Any())
            {
                var uncached = await featureStore_.Value.GetFeaturesAsync(toQuery, services_);
                lock (features_)
                {
                    foreach (var name in names)
                    {
                        if (features_.TryGetValue(name, out var cachedValue))
                        {
                            /// preserve cached as someone else might be already using it
                            result[name] = cachedValue;
                        }
                        else
                        {
                            var value = uncached.GetValueOrDefault(name);
                            features_.Add(name, value);
                            result[name] = value;
                        }
                    }
                }
            }

            return result;

        }

        public async Task<bool> IsFeatureEnabledAsync(string name)
        {
            bool value;
            lock (features_)
            {
                if (features_.TryGetValue(name, out value))
                    return value;
            }

            value = await featureStore_.Value.IsFeatureEnabledAsync(name, services_);
            lock (features_)
            {
                bool dummValue;
                if (features_.TryGetValue(name, out dummValue))
                    return dummValue;
                features_.Add(name, value);
            }
            return value;
        }

        public async Task<bool> IsChaosAsync(string route)
        {
            bool value;
            lock (chaos_)
            {
                if (chaos_.TryGetValue(route, out value))
                    return value;
            }

            value = await featureStore_.Value.IsChaosAsync(route, services_);
            lock (chaos_)
            {
                if (chaos_.TryGetValue(route, out value))
                    return value;
                chaos_.Add(route, value);
            }
            return value;
        }
    }

    public static class IFeatureGateExtension
    {
        public static async Task EnsureFeatureAsync(this IFeatureGate gate, string name)
        {
            if (!await gate.IsFeatureEnabledAsync(name))
            {
                throw new AccessDeniedException($"Feature {name} is disabled");
            }
        }

        public static async Task<bool> IsFeatureEnabledAsync(this IFeatureGate gate, string name)
        {
            return (await gate.IsFeatureEnabledAsync(name));
        }

        public static async Task<bool> IsChaosAsync(this IFeatureGate gate, string route)
        {
            return await gate.IsChaosAsync(route);
        }
    }

    public static class FeatureGateExtension
    {
        /// adds tightly coupled set of features which interdepend on proper IP handling settings
        public static IServiceCollection AddFeatureGates(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:10.0.0.0"), 104));
            });
            services.AddSingleton(services =>
            {
                // TODO: use 2 connections strings to allow for failover on API level in future (for now it is on service level health check traffic routing)
                // TODO: via vnet -> k8s external service https://github.com/crey-games/backend-infrastructure/pull/147#issuecomment-735687669
                var config = services.GetRequiredService<IConfiguration>();
                var cloudStorageAccount =
                    config.GetValue<string>("cosmos-db-table-distributed")
                    .To(CloudStorageAccount.Parse);
                return new EventualCloudTableClient(cloudStorageAccount.TableStorageUri, cloudStorageAccount.Credentials);
            });
            services.AddSingleton<GeoLocationService>();
            services.AddSingletonCreyServiceInternal<GeoLocationQuery>();
            services.AddSingletonCreyServiceInternal<FeatureGateStore>();
            services.AddSingletonCreyServiceInternal<RegressionTest>();
            services.AddScopedCreyService<IFeatureGate, CachedFeatureGate>();
            return services;
        }

        public static async Task DisableFunctionality(this ICreyService<IFeatureGate> gate, string gateName)
        {
            if (await gate.Value.IsFeatureEnabledAsync(gateName))
                throw new HttpStatusErrorException(HttpStatusCode.MethodNotAllowed, $"Feature disabled by {gateName} featuregate");
        }
    }
}
