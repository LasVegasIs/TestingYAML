using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Crey.Authentication
{
    public class CDNServiceRuleValidationMiddleware
    {
        private readonly RequestDelegate next_;

        public CDNServiceRuleValidationMiddleware(RequestDelegate next)
        {
            next_ = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            CDNServiceRuleAttribute.Validate(httpContext);
            await next_(httpContext);
        }
    }

    public static class CDNServiceRuleValidationExtensions
    {
        public static IApplicationBuilder UseCDNServiceRuleValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CDNServiceRuleValidationMiddleware>();
        }
    }
}
