using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Crey.Authentication
{
    public class Service2ServiceValidationMiddleware
    {
        private readonly RequestDelegate next_;

        public Service2ServiceValidationMiddleware(RequestDelegate next)
        {
            next_ = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            ServerToServerAttribute.Validate(httpContext);
            await next_(httpContext);
        }
    }

    public static class Service2ServiceValidationExtensions
    {
        public static IApplicationBuilder UseService2ServiceValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Service2ServiceValidationMiddleware>();
        }
    }
}
