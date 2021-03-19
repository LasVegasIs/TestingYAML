using Crey.Web.Service2Service;
using Microsoft.AspNetCore.Builder;

namespace Crey.Kernel.Authentication
{
    public static class Service2ServiceValidationExtensions
    {
        public static IApplicationBuilder UseService2ServiceValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Service2ServiceValidationMiddleware>();
        }
    }
}
