using Crey.Configuration.ConfigurationExtensions;
using Serilog;
using Serilog.Exceptions.Core;

using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace Microsoft.AspNetCore.Hosting
{
    // note: moved to instrumentation
    public static class CreyLogExtensions
    {
        // Consider adding Slack or other integration as alert hook on AppInsight
        public static IWebHostBuilder ConfigureCreyMicroserviceLogging(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog(
                (hostingContext, loggerConfiguration) =>
                {
                    var configuration = hostingContext.Configuration;
                    var deploymentSlot = configuration.GetDeploymentSlot();

                    loggerConfiguration
                      .ReadFrom.Configuration(hostingContext.Configuration);

                    var aiKey = configuration.GetApplicationInsightKey();
                    if (!string.IsNullOrEmpty(aiKey))
                    {
                        // TODO: as soon as IHostBuilder will be used, use add serilog method which gets telemetry client from services
                        loggerConfiguration.WriteTo
                            .ApplicationInsights(aiKey, new TraceTelemetryConverter(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug);
                    }

                    loggerConfiguration
                      .Enrich.FromLogContext()
                      .Enrich.WithMachineName() // easy to correllate with cloud machine
                      .Enrich.WithThreadId() // background services
                      .Destructure.ToMaximumCollectionCount(42)
                                    .Destructure.ToMaximumStringLength(2048)
                                    .Destructure.ToMaximumDepth(2)
                      .Enrich.WithExceptionDetails(
                                    new DestructuringOptionsBuilder()
                                    .WithDefaultDestructurers()
                                    // next seems very usefull, but mem safe; can try disable it on live if needed
                                    //.WithIgnoreStackTraceAndTargetSiteExceptionFilter()
                                    .WithDestructuringDepth(2) // Aggregate + Inner
                                    .WithoutReflectionBasedDestructurer()  // slow and dangerious, so disabled
                              )
                      .Filter
                          .ByExcluding(Matching.WithProperty<string>("RequestPath", x => x.StartsWith("info/detail") || x.StartsWith("metrics")));
                });
        }
    }
}
