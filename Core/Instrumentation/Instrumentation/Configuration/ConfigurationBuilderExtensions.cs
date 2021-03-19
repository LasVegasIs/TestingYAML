using Crey.Instrumentation.Web;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Rest.TransientFaultHandling;
using System;
using System.IO;

namespace Crey.Instrumentation.Configuration
{

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddCreyConfigurations(this IConfigurationBuilder configurationBuilder, string[] args, string service, string azureDeploymentSlot, KeyVaultPolicy vaultPolicy)
        {
            // extract the minimal info required for early configuration, skip all irrelevant layers
            var earlyConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddEnvironmentVariables("APPSETTING_")
                .AddJsonFile("appsettings.json", optional: false)
                .AddCommandLine(args)
                .Build();

            var deploymentSlot = earlyConfiguration.GetDeploymentSlot();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
            var isCloud = earlyConfiguration.IsRunningInCloud();
            Console.WriteLine($"Prepare configuration for {service} in {environment} environment for {deploymentSlot}...");
            if (string.IsNullOrEmpty(deploymentSlot))
                throw new Exception("For debug build or tool usage set slot from command line (--DeploymentSlot dev) or from env variable (APPSETTING_DeploymentSlot=\"dev\")");

            // add configuration sources
            configurationBuilder.AddEnvironmentVariables("APPSETTING_");

            // add key vault
            var serviceTruncated = service.Substring(0, Math.Min(service.Length, 13));
            var slotTruncated = deploymentSlot.Substring(0, Math.Min(deploymentSlot.Length, 7));
            var sharedKeyVaultAddress = $"https://crey{slotTruncated}.vault.azure.net/";
            var privateKeyVaultAddress = $"https://crey{serviceTruncated}{slotTruncated}.vault.azure.net/";
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            keyVaultClient.SetRetryPolicy(new RetryPolicy<HttpRequestExceptionErrorDetectionStrategy>(
                new IncrementalRetryStrategy(1000, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0))));
            if (vaultPolicy == KeyVaultPolicy.Shared || vaultPolicy == KeyVaultPolicy.All)
                configurationBuilder.AddAzureKeyVault(sharedKeyVaultAddress, keyVaultClient, new DefaultKeyVaultSecretManager());
            if (vaultPolicy == KeyVaultPolicy.Private || vaultPolicy == KeyVaultPolicy.All)
                configurationBuilder.AddAzureKeyVault(privateKeyVaultAddress, keyVaultClient, new DefaultKeyVaultSecretManager());

            // add common configs
            configurationBuilder.AddJsonFile("appsettings.common.json", false); // ! mandatory
            configurationBuilder.AddJsonFile($"appsettings.common.{deploymentSlot}.json", true);

            // service specific configs
            configurationBuilder.AddJsonFile("ratelimit.json", true);
            configurationBuilder.AddJsonFile("appsettings.json", false); // ! mandatory, ex: contains changeset
            configurationBuilder.AddJsonFile($"appsettings.logging.json", true);
            configurationBuilder.AddJsonFile($"appsettings.{deploymentSlot}.json", true);

            //add deprecated configs, service names are removed
            configurationBuilder.AddJsonFile($"appsettings.{service}.json", true)
                .AddJsonFile($"appsettings.logging.{service}.json", true)
                .AddJsonFile($"appsettings.{service}.{deploymentSlot}.json", true)
                .AddJsonFile($"appsettings.logging.{service}.{deploymentSlot}.json", true);

            //local and temp overrides (ex debug and cmd line arguments)
            if (!isCloud) configurationBuilder.AddJsonFile($"appsettings.local.json", true);
            configurationBuilder.AddCommandLine(args);

            var completeConfiguration = configurationBuilder.Build();
            if (deploymentSlot != completeConfiguration.GetDeploymentSlot())
                throw new Exception("Early configuration refers to a different slot");
            completeConfiguration.ValidateConfiguration(azureDeploymentSlot);

            return configurationBuilder;
        }
    }
}
