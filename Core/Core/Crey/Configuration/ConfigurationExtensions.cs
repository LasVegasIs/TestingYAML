using Core.Extension.CreyNamePatterns;
using Crey.Exceptions;
using Crey.Extensions;
using Crey.Kernel.IAM;
using Crey.Kernel.Proxy;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crey.Configuration.ConfigurationExtensions
{
    // note: copied to standard
    public static class ConfigurationExtensions
    {
        public const string ServiceBaseUri = "https://${service}-${stage}.playcrey.com";
        public const string ProxyServiceBaseUri = "https://${stage}.playcrey.com";

        public static readonly string LocalEnvironment = "Development";

        [Obsolete()]
        public static readonly string WebAppEnvironment = "WebApp";
        public static readonly string KubernetesEnvironment = "Kubernetes";

        public static string GetDeploymentSlot(this IConfiguration configuration)
        {
            var slot = configuration.GetValue<string>("DeploymentSlot")?.ToLower()
                ?? throw new ServerErrorException($"Missing deploymentSlot");

            if (slot.Any(x => !char.IsLetter(x)))
            {
                throw new ServerErrorException($"Invalid slot name, slot can contain only alpha (letter) characters and should be lowercase, provided: {slot}");
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

        /// <summary>
        /// Gets stage lower cased.
        /// </summary>
        public static string GetStage(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("stage")?.ToLower()
                ?? configuration.GetValue<string>($"{configuration.GetDeploymentSlot()}:stage")?.ToLower();
        }

        public static (string, string) GetSplitStage(this IConfiguration configuration)
        {
            var stage = configuration.GetStage() ?? throw new ServerErrorException($"Missing stage");

            var id = Array.FindIndex(stage.ToCharArray(), x => !char.IsLetter(x));
            if (id < 0)
                throw new ServerErrorException($"Invalid stage name ({stage}), missing time-stamp");

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

        public static string GetHostingEnvironmentName(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        }

        public static string GetSessionCookieName(this IConfiguration configuration)
        {
            return $"Crey.{configuration.GetDeploymentSlot().Capitalize()}.Session";
        }

        public static string GetCookieDomain(this IConfiguration configuration)
        {
            return ".playcrey.com";
        }

        // TODO: move to IAM
        public static string GetReCaptchaSiteKey(this IConfiguration configuration, string serviceName)
        {
            // do not break current auth recaptcha usage
            if (serviceName == IAMDefaults.SERVICE_NAME && configuration.IsTestingSlot())
            {
                // public test key, see: https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do
                return "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI";
            }

            return configuration.GetValue<string>("GoogleReCaptcha:ClientKey");
        }

        // TODO: move to IAM
        public static string GetReCaptchaSecretKey(this IConfiguration configuration, string serviceName)
        {
            // do not break current auth recaptcha usage
            if (serviceName == IAMDefaults.SERVICE_NAME && configuration.IsTestingSlot())
            {
                // public test key, see: https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do
                return "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
            }

            return configuration.GetValue<string>("GoogleReCaptcha:SecretKey");
        }

        public static string[] GetAllowedReferrers(this IConfiguration configuration)
        {
            string allowedReferrersJsonString = configuration.GetValue<string>("AllowedReferrers");
            var allowedReferrersJson = JObject.Parse(allowedReferrersJsonString);
            return allowedReferrersJson["AllowedReferrers"].ToObject<string[]>();
        }

        public static void ValidateConfiguration(this IConfiguration configuration, string azureDeploymentSlot)
        {
            var deploymentSlot = configuration.GetDeploymentSlot();
            var (stageSlot, stageTimestamp) = configuration.GetSplitStage();
            Console.WriteLine($"Using DB stage: {deploymentSlot}/{stageTimestamp}");

            if (deploymentSlot != stageSlot)
                throw new ServerErrorException($"Stage - Deployment slot ({deploymentSlot}) mismatch with Stage slot ({stageSlot})");

            if (azureDeploymentSlot == "Development")
            {
                if (configuration.IsProductionSlot())
                {
                    throw new ServerErrorException($"Invalid deployment with Deployment Slot {deploymentSlot}!");
                }
            }
        }

        /// TODO: move to proxy config
        /// to be used in all services when we need that URI of the website
        public static string GetWebsiteBaseURI(this IConfiguration configuration)
        {
            var customServiceInfos = configuration.GetCustomServiceInfos();
            customServiceInfos.TryGetValue(ProxyDefaults.SERVICE_NAME, out CustomizedServiceInfo info);

            string baseUriPattern = info?.BaseUri ?? ProxyServiceBaseUri;
            string deploymentSlot = configuration.GetDeploymentSlot();

            return configuration.IsProductionSlot()
                ? baseUriPattern.SubstituteCreyStagePattern("www", "", "")
                : baseUriPattern.SubstituteCreyStagePattern(deploymentSlot.ToString(), "", "");
        }

        // TODO: move it into proxy service only
        public static IEnumerable<string> GetProxyBaseURIs(this IConfiguration configuration)
        {
            var customServiceInfos = configuration.GetCustomServiceInfos();
            customServiceInfos.TryGetValue(ProxyDefaults.SERVICE_NAME, out CustomizedServiceInfo info);

            var baseUriPatterns = new List<string> { ProxyServiceBaseUri };
            if (info == null)
            {
                if (configuration.IsProductionSlot())
                {
                    baseUriPatterns.Add(ProxyServiceBaseUri.SubstituteCreyStagePattern("www", "", ""));
                    baseUriPatterns.Add(ProxyServiceBaseUri.SubstituteCreyStagePattern("", "", ""));
                }
            }
            else
            {
                baseUriPatterns = new List<string> { info.BaseUri };
            }

            string deploymentSlot = configuration.GetDeploymentSlot();
            return baseUriPatterns.Select(baseUriPattern => baseUriPattern.SubstituteCreyStagePattern(deploymentSlot.ToString(), ProxyDefaults.SERVICE_NAME, ""));
        }

        // TODO: move it to proxy service
        public static string GetBaseURI(this IConfiguration configuration, string service)
        {
            var serviceName = service.ToLower();
            if (serviceName == ProxyDefaults.SERVICE_NAME)
            {
                throw new InvalidArgumentException("Use GetProxyBaseURIs instead.");
            }

            var customServiceInfos = configuration.GetCustomServiceInfos();
            customServiceInfos.TryGetValue(service, out CustomizedServiceInfo info);

            string baseUriPattern = info?.BaseUri ?? ServiceBaseUri;
            string deploymentSlot = configuration.GetDeploymentSlot();

            return baseUriPattern.SubstituteCreyStagePattern(deploymentSlot.ToString(), serviceName, "");
        }

        [Obsolete]
        public static string GetChannelName(this IConfiguration configuration, string channel)
        {
            var deploymentSlot = configuration.GetDeploymentSlot();
            return $"{deploymentSlot}-{channel}";
        }

        private class CustomizedServiceInfo
        {
            public string BaseUri { get; set; }
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
    }
}
