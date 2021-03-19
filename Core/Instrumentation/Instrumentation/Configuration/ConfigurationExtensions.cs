using Crey.Instrumentation.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crey.Instrumentation.Configuration
{
    public static class ConfigurationExtensions
    {
        public const string ServiceBaseUri = "https://${service}-${stage}.playcrey.com";
        public const string ProxyServiceBaseUri = "https://${stage}.playcrey.com";

        public static readonly string LocalEnvironment = "Development";
        public static readonly string KubernetesEnvironment = "Kubernetes";

        public static string GetDeploymentSlot(this IConfiguration configuration)
        {
            var slot = configuration.GetValue<string>("DeploymentSlot")?.ToLower()
                ?? throw new InternalServerErrorException($"Missing deploymentSlot");

            if (slot.Any(x => !char.IsLetter(x)))
            {
                throw new InternalServerErrorException($"Invalid slot name, slot can contain only alpha (letter) characters and should be lowercase, provided: {slot}");
            }
            return slot;
        }

        public static string GetChangeset(this IConfiguration self)
        {
            return self.GetValue<string>("Changeset");
        }

        public static bool IsProductionSlot(this IConfiguration configuration)
        {
            return configuration.GetDeploymentSlot() == "live";
        }

        public static bool IsTestingSlot(this IConfiguration configuration)
        {
            return !configuration.IsProductionSlot();
        }

        public static bool IsSandboxed(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Sandboxed");
        }

        

        /// <summary>
        /// Gets stage lower cased.
        /// </summary>
        public static string? GetStage(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("stage")?.ToLower()
                ?? configuration.GetValue<string>($"{configuration.GetDeploymentSlot()}:stage")?.ToLower();
        }

        public static (string, string) GetSplitStage(this IConfiguration configuration)
        {
            var stage = configuration.GetStage() ?? throw new InternalServerErrorException($"Missing stage");

            var id = Array.FindIndex(stage.ToCharArray(), x => !char.IsLetter(x));
            if (id < 0)
                throw new InternalServerErrorException($"Invalid stage name ({stage}), missing time-stamp");

            var deploymentSlot = stage.Substring(0, id);
            var timestamp = stage.Substring(id);
            return (deploymentSlot, timestamp);
        }

        public static bool IsRunningInCloud(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("IsRunningInCloud");
        }

        public static string GetApplicationInsightKey(this IConfiguration config)
        {
            return config.GetHostingEnvironmentName() == KubernetesEnvironment
                ? config.GetValue<string>($"{config.GetValue<string>("Region")}:AppInsightsInstrumentationKey")
                : config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");
        }


        private static string GetHostingEnvironmentName(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        }

        public static string GetSessionCookieName(this IConfiguration configuration)
        {
            return $"Crey.{configuration.GetDeploymentSlot().ToUpperInvariant()}.Session";
        }

        public static string GetCookieDomain(this IConfiguration configuration)
        {
            return ".playcrey.com";
        }

        public static string[]? GetAllowedReferrers(this IConfiguration configuration)
        {
            string allowedReferrersJsonString = configuration.GetValue<string>("AllowedReferrers");
            var allowedReferrersJson = JObject.Parse(allowedReferrersJsonString);
            return allowedReferrersJson["AllowedReferrers"]?.ToObject<string[]>();
        }

        public static void ValidateConfiguration(this IConfiguration configuration, string azureDeploymentSlot)
        {
            var deploymentSlot = configuration.GetDeploymentSlot();
            var (stageSlot, stageTimestamp) = configuration.GetSplitStage();
            Console.WriteLine($"Using DB stage: {deploymentSlot}/{stageTimestamp}");

            if (deploymentSlot != stageSlot)
                throw new InternalServerErrorException($"Stage - Deployment slot ({deploymentSlot}) mismatch with Stage slot ({stageSlot})");

            if (azureDeploymentSlot == "Development")
            {
                if (configuration.IsProductionSlot())
                {
                    throw new InternalServerErrorException($"Invalid deployment with Deployment Slot {deploymentSlot}!");
                }
            }
        }

        public static string GetBaseURI(this IConfiguration configuration, string service)
        {
            var serviceName = service.ToLower();

            var customized = $"{serviceName}BaseUriPattern";
            var baseUriPattern = configuration.GetValue<string>(customized);
            if (string.IsNullOrEmpty(baseUriPattern))
            {
                baseUriPattern = configuration.GetValue<string>("BaseUriPattern");
            }
            if (string.IsNullOrEmpty(baseUriPattern))
            {
                baseUriPattern = ServiceBaseUri;
            }

            string deploymentSlot = configuration.GetDeploymentSlot();
            return baseUriPattern
                .SubstituteStagePattern(deploymentSlot.ToString())
                .SubstituteServicePattern(serviceName);
        }

        private class CustomizedServiceInfo
        {
            public string BaseUri { get; set; } = null!;
        }

        private static Dictionary<string, CustomizedServiceInfo> GetCustomServiceInfos(this IConfiguration configuration)
        {
            var prefix = "service-";
            var customServiceInfos = new Dictionary<string, CustomizedServiceInfo>();

            foreach (var serviceConfig in configuration.GetChildren())
            {
                if (serviceConfig.Key.StartsWith(prefix))
                {
                    var serviceName = serviceConfig.Key.Substring(prefix.Length);
                    var info = new CustomizedServiceInfo { BaseUri = ServiceBaseUri };
                    serviceConfig.Bind(info);
                    customServiceInfos.Add(serviceName, info);
                }
            }

            return customServiceInfos;
        }

        public static string SubstituteStagePattern(this string pattern, string inputStage)
        {
            var stage = inputStage.ToLower();
            var capStage = string.IsNullOrEmpty(stage) ? stage : stage.First().ToString().ToUpper() + stage.Substring(1);
            var negStage = string.IsNullOrEmpty(stage) ? "" : "-" + stage;

            return pattern
                .Replace("${stage}", stage)
                .Replace("${Stage}", capStage)
                .Replace("${-stage}", negStage);
        }

        public static string SubstituteServicePattern(this string pattern, string inputService)
        {
            var service = inputService.ToLower();
            var capService = string.IsNullOrEmpty(service) ? service : service.First().ToString().ToUpper() + service.Substring(1);
            var negService = string.IsNullOrEmpty(service) ? "" : "-" + service;

            return pattern
                .Replace("${service}", service)
                .Replace("${Service}", capService)
                .Replace("${-service}", negService);
        }

        public static string SubstituteNamePattern(this string pattern, string inputName)
        {
            var name = inputName.ToLower();
            var capName = string.IsNullOrEmpty(name) ? name : name.First().ToString().ToUpper() + name.Substring(1);
            var negName = string.IsNullOrEmpty(name) ? "" : "-" + name;

            return pattern
                .Replace("${name}", name)
                .Replace("${Name}", capName)
                .Replace("${-name}", negName);
        }
    }
}
