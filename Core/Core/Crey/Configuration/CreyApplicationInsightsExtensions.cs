using Crey.Configuration.ConfigurationExtensions;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Crey.Configuration
{
    //note: migrated to standard
    public static class CreyApplicationInsightsExtensions
    {
        public static IServiceCollection AddCreyApplicationInsights(this IServiceCollection services, IConfiguration config)
        {
            var aiOptions = new ApplicationInsightsServiceOptions();

            aiOptions.InstrumentationKey = config.GetApplicationInsightKey();

            services.AddHttpContextAccessor();
            //services.AddSingleton<ITelemetryInitializer, CreyTelemetryInitializer>();
            services.AddApplicationInsightsTelemetry(aiOptions);
            return services;
        }
    }

    /*
     * Not working as RequestServices is null by the time telemetry got called :(
     * This code is left here only for a sample.
     * internal class CreyTelemetryInitializer : TelemetryInitializerBase
    {
        public CreyTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
             : base(httpContextAccessor)
        {
        }

        protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
        {
            var serives = platformContext.RequestServices;
            if (serives == null)
                return;

            if (string.IsNullOrEmpty(requestTelemetry.Context.User.Id) || string.IsNullOrEmpty(requestTelemetry.Context.Session.Id))
            {
                var sessionInfo = serives.GetRequiredService<SessionInfoStore>().Value;
                if (sessionInfo != null)
                {
                    requestTelemetry.Context.User.Id = sessionInfo.AccountId.ToString();
                    telemetry.Context.GlobalProperties.Add("Roles", string.Join(",", sessionInfo.Roles));
                }
            }
        }
    }*/
}
