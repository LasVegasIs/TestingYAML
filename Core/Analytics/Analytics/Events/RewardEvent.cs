using Crey.MessageContracts.Notification;
using Crey.MessageContracts.Rewards;
using Newtonsoft.Json;

namespace Analytics.Events
{
#nullable enable
    public sealed class RewardEvent : AnalyticsEvent
    {
        public override string Event => "Item_In";

        [JsonProperty("item_detail")]
        public Reward? Reward { get; set; }

        [JsonProperty("item_trigger")]
        public RewardTrigger? Trigger { get; set; }

        public RewardEvent(int userId, Reward reward, RewardTrigger trigger)
            : base(userId)
        {
            Reward = reward;
            Trigger = trigger;
        }
    }
}
