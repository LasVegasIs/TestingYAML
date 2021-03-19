using Microsoft.Extensions.DependencyInjection;

namespace Crey.Infrastructure
{
    public class ServiceInfo
    {
        public ServiceInfo(string name, string changeset)
        {
            Name = name;
            Changeset = changeset;
        }
        public string Name { get; set; }

        public string Changeset { get; set; }
    }

    public static class ServiceInfoExtensions
    {
        public static IServiceCollection AddServiceInfo(this IServiceCollection services, ServiceInfo serviceInfo)
        {
            return services.AddSingleton(serviceInfo);
        }
    }
}
