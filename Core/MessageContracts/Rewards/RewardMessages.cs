using Crey.MessageContracts.Avatar;
using Crey.MessageContracts.Bank;
using Crey.MessageContracts.GameEvents;
using Crey.MessageContracts.Notification;
using Crey.MessageContracts.Prefabs;
using Crey.MessageContracts.UserProfile;
using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Rewards
{
    [MessageTopic("reward")]
    [JsonConverter(typeof(JsonMessageSerdeConverter<RewardMessage>))]
    public abstract class RewardMessage : IMessageContract
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        public int RewardedAccountId { get; set; }
        public RewardTrigger Trigger { get; set; }
        public NotificationType NotificationType { get; set; }

        internal RewardMessage() { }
    }

    public abstract class RewardMessage<TReward> : RewardMessage
        where TReward : Reward
    {
        public TReward Reward { get; set; }
    }
}
