using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Extensions;
using Microsoft.Extensions.Configuration;

namespace Crey.Web.Analytics
{
    public static class AnalyticsExtensions
    {
        public static string GetGoogleAnalyticsTrackingId(this IConfiguration configuration, SessionInfo sessionInfo)
        {
            return configuration.GetValue<string>("GoogleAnalyticsTrackingId")
                ?? configuration.GetValue<string>($"{configuration.GetDeploymentSlot()}:GoogleAnalyticsTrackingId");
        }

        public static string GetTrackingIdCookieName(this IConfiguration configuration)
        {
            return $"Crey.{configuration.GetDeploymentSlot().Capitalize()}.TrackingId";
        }
    }
}