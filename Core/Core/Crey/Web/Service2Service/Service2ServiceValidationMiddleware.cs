using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Crey.Web.Service2Service
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
            CDNServiceRuleAttribute.Validate(httpContext);

            await next_(httpContext);
        }
    }
}
