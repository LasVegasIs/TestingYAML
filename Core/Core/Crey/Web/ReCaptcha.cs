using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Kernel;
using Crey.Kernel.IAM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crey.Web
{
    public class ReCaptcha
    {
        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            [JsonProperty("error-codes")]
            public string[] ErrorCodes { get; set; }
        }

        private readonly IHttpClientFactory httpClientFactory_;
        private readonly IConfiguration configuration_;
        private readonly ServiceOption serviceOption_;
        private readonly ILogger<ReCaptcha> logger_;

        public ReCaptcha(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ServiceOption serviceOption,
            ILogger<ReCaptcha> logger)
        {
            httpClientFactory_ = httpClientFactory;
            configuration_ = configuration;
            serviceOption_ = serviceOption;
            logger_ = logger;
        }

        public async Task<bool> IsResponseValid(string response)
        {
            string reCaptchaSecretKey = configuration_.GetReCaptchaSecretKey(serviceOption_.Service);
            var httpClient = httpClientFactory_.CreateClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={reCaptchaSecretKey}&response={response}");
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                ReCaptchaResponse reCaptchaResponse = await httpResponse.Content.ReadAsAsync<ReCaptchaResponse>();
                if (reCaptchaResponse.Success)
                {
                    return true;
                }

                logger_.LogError($"ReCaptcha validation failed with errorcodes: {string.Join(',', reCaptchaResponse.ErrorCodes)}");
                return false;
            }

            logger_.LogError($"Calling ReCaptcha API failed with response code: {httpResponse.StatusCode.ToString()}");
            return false;
        }
    }

    public static class ReCaptchaExtensions
    {
        public static IServiceCollection AddReCaptcha(this IServiceCollection collectionBuilder)
        {
            //Debug.Assert(collectionBuilder.HasKernelConfiguration());
            collectionBuilder.TryAddSingleton<ReCaptcha>();
            return collectionBuilder;
        }
    }
}
