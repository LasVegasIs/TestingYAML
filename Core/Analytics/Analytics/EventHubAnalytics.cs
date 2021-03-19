#nullable enable

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics
{
    public class EventHubAnalytics : IAnalytics
    {
        private readonly EventHubProducerClient? producerClient_;
        private readonly string slot_;

        public EventHubAnalytics(IConfiguration configuration)
        {
            var cns = configuration.GetAnalyticsEventHubConnectionString();
            if(!string.IsNullOrEmpty(cns))
                producerClient_ = new EventHubProducerClient(configuration.GetAnalyticsEventHubConnectionString());
            slot_ = configuration.GetDeploymentSlot();
        }

        public async Task SendEventAsync(IEnumerable<AnalyticsEvent> evt)
        {
            if (producerClient_ == null || producerClient_.IsClosed)
                return;

            var evtDatas = evt.Select(x =>
            {
                x.DeploymentSlot = slot_; // enforce stage by the config
                return new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(x)));
            });
            await producerClient_.SendAsync(evtDatas);
        }
    }

    public static class EventHubAnalyticsExtensions
    {
        public static string GetAnalyticsEventHubConnectionString(this IConfiguration configuration)
        {
            return configuration.GetValue<string>($"creyeventhub");
        }

        public static IServiceCollection AddEventHubAnalytics(this IServiceCollection services)
        {
            return services.AddSingleton<IAnalytics, EventHubAnalytics>();
        }
    }
}
