using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MessageContracts.Rewards
{
    public class AvatarReward : IReward, IRewardDetail
    {
        public const string RewardType = "AvatarReward";
        public string Type => RewardType;

        [MinLength(1, ErrorMessage = "AvatarIds must contain at least 1 element")]
        [MaxLength(10, ErrorMessage = "AvatarIds cannot contain more than 10 elements")]
        public List<int> AvatarIds { get; set; } = new List<int>();
    }

    public class AvatarRewardMessage : RewardMessage<AvatarReward> { }
}
