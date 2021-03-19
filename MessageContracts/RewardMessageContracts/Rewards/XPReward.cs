using System.ComponentModel.DataAnnotations;

namespace MessageContracts.Rewards
{
    public class XPReward : IReward, IRewardDetail
    {
        public const string RewardType = "XPReward";
        public string Type => RewardType;

        [Range(1, 10000, ErrorMessage = "Amount exceeds limits (1..10000)")]
        public uint Amount { get; set; }
    }

    public class XPRewardMessage : RewardMessage<XPReward> { }
}
