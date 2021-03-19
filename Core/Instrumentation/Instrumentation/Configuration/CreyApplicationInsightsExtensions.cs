using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Instrumentation.Configuration
{
    public static class ApplicationInsightsExtensions
    {
        public static IServiceCollection AddCreyApplicationInsights(this IServiceCollection services, IConfiguration config)
        {
            var aiOptions = new ApplicationInsightsServiceOptions();

            aiOptions.InstrumentationKey = config.GetApplicationInsightKey();

            services.AddHttpContextAccessor();
            services.AddApplicationInsightsTelemetry(aiOptions);
            return services;
        }
    }
}
