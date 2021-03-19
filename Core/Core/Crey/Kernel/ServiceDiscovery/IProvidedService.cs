using Core.Functional;
using Crey.Contracts;

namespace Crey.Kernel.ServiceDiscovery
{
    public interface IProvidedService
    {
        Result<ServiceInfo, Error> GetInfo();
        Result<string, Error> GetServiceURI(string service);
    }
}
