using MessageContracts.Rewards;
using MessagingCore;

namespace MessageContracts
{
    public interface IRewardMessage : IMessageContract
    {
        public const string TOPIC = "reward";
    }

    public abstract class RewardMessage<TReward> : IRewardMessage
        where TReward : IReward
    {
        public string Type => $"{Reward.Type}";

        public int RewardedAccountId { get; set; }
        public RewardNotificationOptions NotificationOptions { get; set; }

        public TReward Reward { get; set; }
    }
}
