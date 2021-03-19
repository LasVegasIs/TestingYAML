using Crey.Configuration.ConfigurationExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Rest.TransientFaultHandling;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using ConfigurationExtensions = Crey.Configuration.ConfigurationExtensions.ConfigurationExtensions;

// moved to netstandard
namespace Crey.Web
{

    public enum KeyVaultPolicy
    {
        Shared,
        Private,
        All,
    }

    public static class WebHostExtensions
    {
        private static readonly string CertFile = "../../tmp/playcrey-com.pfx";

        public static IWebHostBuilder CreyConfigureAppConfiguration(this IWebHostBuilder webHost, string service, KeyVaultPolicy vaultPolicy, string[] args)
        {
            return webHost.ConfigureAppConfiguration((context, config) =>
            {
                config.ConfigureService(args, webHost, service, context.HostingEnvironment.EnvironmentName, vaultPolicy);
            });
        }

        private static void SetupLocalCertificationSettings(IWebHostBuilder webHostBuilder)
        {
            var file = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, CertFile));
            if (File.Exists(file))
            {
                var certificate = new X509Certificate2(file);
                webHostBuilder.UseKestrel(
                options =>
                {
                    var port = webHostBuilder.GetSetting("HTTPS_PORT");
                    options.Listen(IPAddress.Loopback, int.Parse(port), listenOptions =>
                    {
                        listenOptions.UseHttps(certificate);
                    });
                });
            }
            else
            {
                Console.WriteLine("Local cert not found! Please download it from Azure Portal to your tmp folder and try again or you may have cert issues!");
            }
        }

        public static IConfigurationBuilder ConfigureService(this IConfigurationBuilder configurationBuilder, string[] args, IWebHostBuilder webHost, string service, string azureDeploymentSlot, KeyVaultPolicy vaultPolicy)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

            configurationBuilder.AddEnvironmentVariables("APPSETTING_");
            configurationBuilder.AddJsonFile("ratelimit.json", true);

            configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddCommandLine(args);

            var earlyConfiguration = configurationBuilder.Build();
            string slot = earlyConfiguration.GetDeploymentSlot();
            string serviceTruncated = service.Substring(0, Math.Min(service.Length, 13));
            string slotTruncated = slot.Substring(0, Math.Min(slot.Length, 7));
            string sharedKeyVaultAddress = $"https://crey{slotTruncated}.vault.azure.net/";
            string privateKeyVaultAddress = $"https://crey{serviceTruncated}{slotTruncated}.vault.azure.net/";

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            keyVaultClient.SetRetryPolicy(new RetryPolicy<HttpRequestExceptionErrorDetectionStrategy>(
                new IncrementalRetryStrategy(1000, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0))));
            if (vaultPolicy == KeyVaultPolicy.Shared || vaultPolicy == KeyVaultPolicy.All)
                configurationBuilder.AddAzureKeyVault(sharedKeyVaultAddress, keyVaultClient, new DefaultKeyVaultSecretManager());
            if (vaultPolicy == KeyVaultPolicy.Private || vaultPolicy == KeyVaultPolicy.All)
                configurationBuilder.AddAzureKeyVault(privateKeyVaultAddress, keyVaultClient, new DefaultKeyVaultSecretManager());


            // CHECK: ASP.NET provides intialized DiagnosticListener - use it instead of console if Console is always presented 
            // OR add Console sink in basic startup (it may log into ASP.NET startup file sink either)
            Console.WriteLine($"Building configuration for {service} in {environment} environment...");
            var configuration = configurationBuilder.Build();

            if (string.IsNullOrEmpty(configuration.GetDeploymentSlot()))
                throw new Exception("For debug build or tool usage set slot from command line (--DeploymentSlot dev) or from env varibale (APPSETTING_DeploymentSlot=\"dev\")");

            var deploymentSlot = configuration.GetDeploymentSlot();
            var normalizedSlotName = deploymentSlot.ToString().ToLower();

            bool isRunningInCloud = configuration.IsRunningInCloud();
            var coreLoggingConfig = "Core/appsettings.logging.json";
            var coreSlotLoggingConfig = $"Core/appsettings.logging.{normalizedSlotName}.json";
            if (isRunningInCloud)
            {
                configurationBuilder.AddJsonFile(coreLoggingConfig, true);
                configurationBuilder.AddJsonFile(coreSlotLoggingConfig, true);
            }
            else
            {
                if (webHost != null && environment == ConfigurationExtensions.LocalEnvironment)
                    SetupLocalCertificationSettings(webHost);

                var current = Directory.GetCurrentDirectory();
                var path = current + $"/../../tmp/secrets/{service}/appsettings.{normalizedSlotName}.json";
                configurationBuilder.AddJsonFile(path, true);

                configurationBuilder.AddJsonFile(Path.Combine(current, "../Core/Core/", coreLoggingConfig), true);
                configurationBuilder.AddJsonFile(Path.Combine(current, "../Core/Core/", coreSlotLoggingConfig), true);
            }

            // allow overrides: 0. core (above) 1. per slot (above) 2. per service tuning. 3. per service and slot
            configurationBuilder.AddJsonFile($"appsettings.logging.{service}.json", true);
            configurationBuilder.AddJsonFile($"appsettings.logging.{service}.{normalizedSlotName}.json", true);

            var completeConfiguration = configurationBuilder.Build();
            completeConfiguration.ValidateConfiguration(azureDeploymentSlot);
            return configurationBuilder;
        }
    }
}
