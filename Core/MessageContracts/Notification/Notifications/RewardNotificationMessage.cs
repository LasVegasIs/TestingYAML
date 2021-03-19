using Crey.MessageContracts.Rewards;

namespace Crey.MessageContracts.Notification
{
    [MessageSerde("RewardNotification")]
    public sealed class RewardNotification : NotificationPayload
    {
        public string TrackingId { get; set; } // unique id, correlating to the CollectedRewards
        public Reward Reward { get; set; }
        public RewardTrigger Trigger { get; set; }
    }

    [MessageSerde("RewardNotification")]
    public sealed class RewardNotificationMessage : NotificationMessage<RewardNotification>
    {
    }
}