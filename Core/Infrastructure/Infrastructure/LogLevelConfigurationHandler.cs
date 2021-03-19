using System;
using System.Linq;
using System.Threading.Tasks;
using Crey.Instrumentation.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using StackExchange.Redis;

namespace Crey.Infrastructure
{
    public class LogLevelConfigurationHandler : IRedisKeySubscriptionHandler
    {
        public Task Handle(string notificationType, IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            if (notificationType == "set")
            {
                var connection = ConnectionMultiplexer.Connect(configuration.GetSharedRedisConnectionString());
                var storedLogLevel = connection.GetDatabase().StringGet(configuration.GetLogLevelRedisKey(serviceProvider));
                if (Enum.TryParse<LogEventLevel>(storedLogLevel, true, out var minimumLevel))
                {
                    CreyLogExtensions.GlobalLogLevelSwitch.MinimumLevel = minimumLevel;
                }
                else
                {
                    CreyLogExtensions.GlobalLogLevelSwitch.MinimumLevel = CreyLogExtensions.GLOBAL_DEFAULT_LOG_LEVEL;
                }
            }

            return Task.CompletedTask;
        }
    }

    public static class LogLevelConfigurationExtensions
    {
        public static IServiceCollection AddLogLevelConfigurationSubscriber(this IServiceCollection services, IConfiguration configuration)
        {
            if (!services.Any(d => d.ServiceType == typeof(ServiceInfo)))
            {
                throw new Exception("ServiceInfo was not registered");
            }

            return services.AddRedisKeySubscriber(options => options.ConnectionString = configuration.GetSharedRedisConnectionString());
        }

        public static IApplicationBuilder AddLogLevelConfigurationSubscriptionHandler(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            return applicationBuilder.RegisterRedisKeySubscriptionHandler<LogLevelConfigurationHandler>(
                configuration.GetLogLevelRedisKey(applicationBuilder.ApplicationServices));
        }
    }
}