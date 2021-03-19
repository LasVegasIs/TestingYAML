using System.ComponentModel.DataAnnotations;

namespace MessageContracts.Rewards
{
    public class EventPointsReward : IReward, IRewardDetail
    {
        public const string RewardType = "EventPointsReward";
        public string Type => RewardType;

        // nullable as sometimes it is deduced from the context - should we have two message ?
        public long? EventId;

        [Range(1, uint.MaxValue, ErrorMessage = "Amount exceeds limits")]
        public uint Amount { get; set; }
    }

    public class EventPointsRewardMessage : RewardMessage<EventPointsReward> { }
}
