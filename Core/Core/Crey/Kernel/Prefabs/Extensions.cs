using Crey.Contracts.Prefabs;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Crey.Kernel.Prefabs
{
    public static class Extensions
    {
        public static IServiceCollection AddPrefabsServices(this IServiceCollection collectionBuilder)
        {
            Debug.Assert(collectionBuilder.HasIDInfoAccessor());

            collectionBuilder
                .AddCreyRestClientFactory()
                .AddScopedCreyService<IPrefabsRepository, RestPrefabsRepository>();
            return collectionBuilder;
        }
    }
}
