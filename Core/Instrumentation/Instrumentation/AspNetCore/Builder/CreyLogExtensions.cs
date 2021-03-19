using Serilog.Core.Enrichers;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Enrichers.AspNetCore.HttpContext;
using Microsoft.AspNetCore.Builder;

namespace Crey.Instrumentation.AspNetCore.Builder
{
    public static class CreyLogExtensions
    {
        public static void UseCreyLogging(this IApplicationBuilder app)
        {
            app.UseSerilogLogContext(options =>
            {
                options.EnrichersForContextFactory = context =>
                {
                    var telemetry = context.Features.Get<RequestTelemetry>();
                    string? clientIp = null;
                    string? roles = null;
                    telemetry?.Context?.GlobalProperties.TryGetValue("ClientIp", out clientIp);
                    telemetry?.Context?.GlobalProperties.TryGetValue("Roles", out roles);
                    return new[]
                      {
                        new PropertyEnricher("TraceIdentifier", context.TraceIdentifier),
                        new PropertyEnricher("ClientIp", clientIp),
                        new PropertyEnricher("Roles",roles),
                    };
                };
            });
        }
    }
}
