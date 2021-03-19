using Crey.Configuration.ConfigurationExtensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crey.Kernel.ServiceDiscovery
{
    public static class RegisterToGateway
    {
        public static async Task RegisterAsync(
            bool isDev,
            IConfiguration configuration,
            ServiceOption serviceOption,
            IHttpClientFactory httpClientFactory)
        {
            if (isDev)
                return;

            string requestSafeString = Regex.Replace(serviceOption.Changeset, "\\@.*$", "");
            string uri = $"https://creypipeline.azurewebsites.net/api/deployed/{serviceOption.Service}/{configuration.GetDeploymentSlot()}/{requestSafeString}";
            // for now it is a basic registration into some function api @Baloo

            var httpClient = httpClientFactory.CreateClient();
            var res = await httpClient.PostAsync(uri, null);
            var msg = await res.Content.ReadAsStringAsync();
            //Console.WriteLine(msg);
        }
    }
}
