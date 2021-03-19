using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MessageContracts.Rewards
{
    public class PrefabPackReward : IReward, IRewardDetail
    {
        public const string RewardType = "PrefabPackReward";
        public string Type => RewardType;

        [MinLength(1, ErrorMessage = "PackIds must contain at least 1 element")]
        [MaxLength(10, ErrorMessage = "PackIds cannot contain more than 10 elements")]
        public List<int> PackIds { get; set; } = new List<int>();
    }

    public class PrefabPackRewardMessage : RewardMessage<PrefabPackReward> { }
}