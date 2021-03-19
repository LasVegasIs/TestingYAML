using MessagingCore;
using MessageContracts.Rewards;
using System.Text.Json.Serialization;

namespace MessageContracts
{
    public interface IRewardNotificationMessage : IMessageContract
    {
        public const string TOPIC = "reward-notification";
    }

    public class RewardNotificationMessage : IRewardNotificationMessage
    {
        public string Type => $"RewardNotification:{Reward.Type}";

        public int RewardedAccountId { get; set; }
        public RewardNotificationOptions NotificationOptions { get; set; }

        [JsonConverter(typeof(RewardConverter))]
        public IRewardDetail Reward { get; set; }
    }

}
