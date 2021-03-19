using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Crey.Web
{
    [Obsolete("Use AnalyticsClient instead")]
    public class GoogleAnalytics
    {
        private readonly IHttpClientFactory httpClientFactory_;
        private readonly Dictionary<string, string> commonParameters_;

        public GoogleAnalytics(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            httpClientFactory_ = httpClientFactory;
            commonParameters_ = new Dictionary<string, string>
            {
                { "v", "1" },   // Version.
                { "tid", configuration.GetValue<string>("GoogleAnalyticsTrackingId") },  // Tracking ID / Property ID.
                { "cid", "555" },   // Anonymous Client ID.
            };
        }

        public async Task TrackScreenAsync(string appName, string appVersion, string appId, string appInstallerId, string screenName)
        {
            var input = new Dictionary<string, string>
            {
                { "t", "screenview" },  // Screenview hit type.
                { "an", appName },  // App name.
                { "av", appVersion },   // App version.
                { "aid", appId },   // App Id.
                { "aiid", appInstallerId }, // App Installer Id.
                { "cd", screenName }  // Screen name / content description.
            };

            await PostToGoogle(input);
        }

        public async Task TrackExceptionAsync(string description, bool isFatal)
        {
            var input = new Dictionary<string, string>
            {
                { "t", "exception" },  // Exception hit type.
                { "exd", description },  // Exception description.
                { "exf", isFatal ? "1" : "0" },   // Exception is fatal?
            };

            await PostToGoogle(input);
        }

        private async Task PostToGoogle(Dictionary<string, string> input)
        {
            var httpClient = httpClientFactory_.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsync(
                "http://www.google-analytics.com/collect",
                new FormUrlEncodedContent(commonParameters_.Concat(input).ToDictionary(x => x.Key, x => x.Value)));
        }
    }
}
