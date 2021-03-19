using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Extensions.DateTimeExtensions;
using Crey.Extensions;
using Crey.Kernel.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Crey.Web.Analytics
{
    public class AnalyticsClient
    {
        private readonly IHttpClientFactory httpClientFactory_;
        private readonly IConfiguration configuration_;
        private readonly IIDInfoAccessor idInfoAccessor_;
        private readonly IHttpContextAccessor httpContextAccessor_;

        public AnalyticsClient(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IIDInfoAccessor idInfoAccessor,
            IHttpContextAccessor httpContextAccessor)
        {
            httpClientFactory_ = httpClientFactory;
            configuration_ = configuration;
            idInfoAccessor_ = idInfoAccessor;
            httpContextAccessor_ = httpContextAccessor;
        }

        public void TrackEvent(string category, string action, string label = "", string value = "")
        {
            var cookieCollection = httpContextAccessor_.HttpContext.Request.Cookies;

            string cliendId = "555"; // Anonymous Client ID. https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
            string gaCookie = "";
            if (cookieCollection.TryGetValue("_ga", out gaCookie))
            {
                cliendId = gaCookie.Substring(6);
            }

            string userId = "";
            var sessionInfo = idInfoAccessor_.GetSessionInfo();
            if (sessionInfo.IsValid)
            {
                userId = sessionInfo.AccountId.ToString();
            }
            else
            {
                string trackingId = "";
                if (cookieCollection.TryGetValue(configuration_.GetTrackingIdCookieName(), out trackingId))
                {
                    userId = trackingId;
                }
            }

            var input = new Dictionary<string, string>
            {
                { "v", "1" },
                { "tid", configuration_.GetGoogleAnalyticsTrackingId(sessionInfo) },
                { "cid", cliendId },
                { "uid", userId },
                { "time", DateTime.Now.ToIsoString() },
                { "t", "event" },
                { "ec", category },
                { "ea", action },
                { "el", label },
                { "ev", value },
            };

            PostToGoogle(input);
            PostToCrey(input);
        }

        private void PostToGoogle(Dictionary<string, string> input)
        {
            Task.Run(async () =>
            {
                var httpClient = httpClientFactory_.CreateClient();
                var _ = await httpClient.PostAsync("https://www.google-analytics.com/collect", new FormUrlEncodedContent(input));
            }).FireAndForgetSafeAsync();
        }

        private void PostToCrey(Dictionary<string, string> input)
        {
            Task.Run(async () =>
            {
                var httpClient = httpClientFactory_.CreateClient();
                var _ = await httpClient.PostAsync("https://creymetrics.azurewebsites.net/api/collect", new FormUrlEncodedContent(input));
            }).FireAndForgetSafeAsync();
        }
    }
}
