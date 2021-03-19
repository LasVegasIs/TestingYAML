using System.Collections.Generic;
using System.Threading.Tasks;

namespace Analytics
{
#nullable enable
    public interface IAnalytics
    {
        Task SendEventAsync(IEnumerable<AnalyticsEvent> evt);
    }

    public static class IAnalyticsExtension
    {
        public static Task SendEventAsync(this IAnalytics analytics, AnalyticsEvent evt)
        {
            return analytics.SendEventAsync(new[] { evt });
        }
    }
}