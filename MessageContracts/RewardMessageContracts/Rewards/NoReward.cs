using System.ComponentModel.DataAnnotations;

namespace MessageContracts.Rewards
{
    /// Reward that grants nothing. 
    public class NoReward : IReward, IRewardDetail
    {
        public const string RewardType = "NoReward";
        public string Type => RewardType;
    }

    public class NoRewardMessage : RewardMessage<NoReward> { }
}