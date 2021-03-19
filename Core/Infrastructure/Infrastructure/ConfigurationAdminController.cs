using System;
using Crey.Authentication;
using Crey.Instrumentation.AspNetCore.Hosting;
using Crey.Instrumentation.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using StackExchange.Redis;

namespace Crey.Infrastructure
{
    public static class ConfigurationRoles
    {
        public const string ConfigurationAdmin = "ConfigurationAdmin";
    }

    [AuthenticateStrict]
    [AuthorizeCrey(Roles = ConfigurationRoles.ConfigurationAdmin)]
    [ApiController]
    public class ConfigurationAdminController: ControllerBase
    {
        [HttpGet("/logging/api/v1/minlevel")]
        public string GetLogLevel()
        {
            return CreyLogExtensions.GlobalLogLevelSwitch.MinimumLevel.ToString();
        }

        
        // Verbose = 0        //     Anything and everything you might want to know about a running block of code.
        // Debug = 1          //     Internal system events that aren't necessarily observable from the outside.
        // Information = 2    //     The lifeblood of operational intelligence - things happen.
        // Warning = 3        //     Service is degraded or endangered.
        // Error = 4          //     Functionality is unavailable, invariants are broken or data is lost.
        // Fatal = 5          //     If you have a pager, it goes off when one of these occurs.
        [HttpPost("/logging/api/v1/minlevel")]
        public void SetLogLevel([FromServices] IConfiguration configuration, string logEventLevel)
        {
            if (Enum.TryParse<LogEventLevel>(logEventLevel, true, out var minimumLevel))
            {
                var connection = ConnectionMultiplexer.Connect(configuration.GetSharedRedisConnectionString());
                connection.GetDatabase().StringSet(configuration.GetLogLevelRedisKey(HttpContext.RequestServices), minimumLevel.ToString());
            }
        }
    }

    public static class ConfigurationAdminExtensions
    {
        public static string GetSharedRedisConnectionString(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("SharedRedisCns");
        }

        public static string GetLogLevelRedisKey(this IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var serviceInfo = serviceProvider.GetRequiredService<ServiceInfo>();
            return $"{configuration.GetDeploymentSlot()}-{serviceInfo.Name}-loglevel";
        }
    }
}