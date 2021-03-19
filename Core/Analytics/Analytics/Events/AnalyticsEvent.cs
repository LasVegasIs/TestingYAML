using Newtonsoft.Json;
using System;

namespace Analytics
{
#nullable enable
    public abstract class AnalyticsEvent
    {
        [ThreadStatic] static private Random? _rnd;
        private static int NextRandom()
        {
            if (_rnd == null)
                _rnd = new Random();
            return _rnd.Next();
        }


        [JsonProperty("source")]
        public string Source => "Backend";

        [JsonProperty("event")]
        public abstract string Event { get; }

        [JsonProperty("stage")]
        public string? DeploymentSlot { get; internal set; }

        [JsonProperty("local_timestamp")]
        public DateTime LocalTimestamp { get; private set; }

        [JsonProperty("userid")]
        public int UserId { get; set; }

        [JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
        public string? Platform { get; set; }

        [JsonProperty("unique_id")]
        public string UniqueId { get; private set; }

        internal AnalyticsEvent(int userId)
        {
            LocalTimestamp = DateTime.UtcNow;
            UniqueId = $"{LocalTimestamp.Ticks}-{NextRandom()}";
            UserId = userId;
        }
    }
}
