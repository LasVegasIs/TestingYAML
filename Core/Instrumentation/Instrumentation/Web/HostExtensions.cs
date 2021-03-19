using Crey.Instrumentation.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Rest.TransientFaultHandling;
using System;
using System.IO;
using ConfigurationExtensions = Crey.Instrumentation.Configuration.ConfigurationExtensions;

namespace Crey.Instrumentation.Web
{

    public static class HostExtensions
    {
        public static IHostBuilder CreyConfigureAppConfiguration(this IHostBuilder webHost, string service, KeyVaultPolicy vaultPolicy, string[] args)
        {
            return webHost.ConfigureAppConfiguration((context, config) =>
            {
                config.AddCreyConfigurations(args, service, context.HostingEnvironment.EnvironmentName, vaultPolicy);
            });
        }
    }
}
