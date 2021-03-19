using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Kernel.ServiceDiscovery
{
    public static class Extensions
    {
        public static IServiceCollection AddProvidedService(this IServiceCollection collectionBuilder, IConfiguration configuration)
        {
            collectionBuilder.AddSingletonCreyService<IProvidedService, ProvidedService>();
            return collectionBuilder;
        }
    }
}
