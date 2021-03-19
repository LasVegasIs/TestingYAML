using Crey.Contracts;
using Crey.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Crey.Kernel.Authentication
{
    public interface IIDInfoAccessor
    {
        SessionInfo GetSessionInfo();
    }

    public static class IDInfoAccessorExtension
    {
        public static bool HasIDInfoAccessor(this IServiceCollection collectionBuilder)
        {
            return collectionBuilder.Any(x => x.ServiceType == typeof(IIDInfoAccessor));
        }
    }
}
