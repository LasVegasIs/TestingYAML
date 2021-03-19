using Serilog;
using Serilog.Exceptions.Core;

using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using Microsoft.AspNetCore.Hosting;
using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Core;
using Serilog.Events;

namespace Crey.Instrumentation.AspNetCore.Hosting
{
    public static class CreyLogExtensions
    {
        public static LogEventLevel GLOBAL_DEFAULT_LOG_LEVEL = LogEventLevel.Warning;
        public static readonly LoggingLevelSwitch GlobalLogLevelSwitch = new LoggingLevelSwitch(GLOBAL_DEFAULT_LOG_LEVEL);

        public static IWebHostBuilder ConfigureCreyMicroserviceLogging(this IWebHostBuilder hostBuilder, LogEventLevel? globalDefaultLogLevel)
        {
            if (globalDefaultLogLevel.HasValue)
            {
                GLOBAL_DEFAULT_LOG_LEVEL = globalDefaultLogLevel.Value;
            }

            return hostBuilder.UseSerilog(
                (hostingContext, loggerConfiguration) =>
                {
                    var configuration = hostingContext.Configuration;
                    var deploymentSlot = configuration.GetDeploymentSlot();

                    loggerConfiguration
                      .MinimumLevel.ControlledBy(GlobalLogLevelSwitch)
                      .ReadFrom.Configuration(hostingContext.Configuration);

                    Configure(loggerConfiguration, configuration);
                });
        }

        public static IHostBuilder ConfigureCreyMicroserviceLogging(this IHostBuilder hostBuilder, LogEventLevel? globalDefaultLogLevel)
        {
            if (globalDefaultLogLevel.HasValue)
            {
                GLOBAL_DEFAULT_LOG_LEVEL = globalDefaultLogLevel.Value;
            }

            return hostBuilder.UseSerilog(
                (hostingContext, loggerConfiguration) =>
                {
                    var configuration = hostingContext.Configuration;
                    var deploymentSlot = configuration.GetDeploymentSlot();

                    loggerConfiguration
                      .MinimumLevel.ControlledBy(GlobalLogLevelSwitch)
                      .ReadFrom.Configuration(hostingContext.Configuration);

                    Configure(loggerConfiguration, configuration);
                });
        }

        private static void Configure(LoggerConfiguration loggerConfiguration, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var aiKey = configuration.GetApplicationInsightKey();
            if (!string.IsNullOrEmpty(aiKey))
            {
                // TODO: as soon as IHostBuilder will be used, use add serilog method which gets telemetry client from services
                loggerConfiguration
                    .WriteTo
                    .ApplicationInsights(aiKey, new TraceTelemetryConverter());
            }

            loggerConfiguration
              .Enrich.FromLogContext()
              .Enrich.WithMachineName() // easy to correllate with cloud machine
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
        }
    }
}
