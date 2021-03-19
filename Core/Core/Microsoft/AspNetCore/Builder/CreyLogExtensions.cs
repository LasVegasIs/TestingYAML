using Serilog.Core.Enrichers;
using Serilog.Enrichers.AspNetCore.HttpContext;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.AspNetCore.Builder
{
    // NOTE: moved to shared lib
    public static class CreyLogExtensions
    {
        public static void UseCreyLogging(this IApplicationBuilder app)
        {
            // we rely on azure appinsight integration so that some request information is logged already and errors either (also may be not so rich as possible here)
            //app.UseSerilogRequestLogging(x =>
            //{
            //    x.MessageTemplate = "{RequestMethod} {RequestPath} {StatusCode} {Elapsed:0.0000}ms";
            //    x.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            //    {
            //        diagnosticContext.Set("UserId", httpContext.User?.IntoSessionInfo(Crey.Contracts.DeploymentSlot.Dev).AccountId);
            //    };
            //});

            app.UseSerilogLogContext(options =>
            {
                options.EnrichersForContextFactory = context =>
                {
                    var telemetry = context.Features.Get<RequestTelemetry>();
                    string clientIp = null;
                    string roles = null;
                    telemetry?.Context?.GlobalProperties.TryGetValue("ClientIp", out clientIp);
                    telemetry?.Context?.GlobalProperties.TryGetValue("Roles", out roles);
                    // AppInsights provides some id already so we do not do next:
                    // var userId = telemetry?.Context?.User.Id;
                    return new[]
                      {

                        new PropertyEnricher("TraceIdentifier", context.TraceIdentifier),
                        // ParentId and OperationId are added by AI, so no need to handle them thre
                        //new PropertyEnricher("X-Request-ID", context.Request.Headers["X-Request-ID"]), 
                        // ClientIp is the way we log X-Forwared-For, but that misses trace of proxies
                        //new PropertyEnricher("X-Forwarded-For", context.Request.Headers["X-Forwarded-For"]),
                        new PropertyEnricher("ClientIp", clientIp),
                        new PropertyEnricher("Roles",roles),
                    };
                };
            });
        }
    }
}
