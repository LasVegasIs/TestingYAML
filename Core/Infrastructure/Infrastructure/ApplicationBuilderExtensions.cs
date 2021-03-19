using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Crey.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCreyHealthChecks(this IApplicationBuilder builder)
        {
            return builder.UseHealthChecks("/info/ready");
        }

        public static IApplicationBuilder UseCreyInfrastructure(this IApplicationBuilder builder)
        {
            return builder.UseCreyHealthChecks().UseHttpMetrics();
        }
    }
}