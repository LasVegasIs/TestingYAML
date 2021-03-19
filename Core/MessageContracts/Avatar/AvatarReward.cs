using Crey.MessageContracts.Rewards;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.Avatar
{
    [MessageSerde("AvatarReward")]
    public sealed class AvatarReward : Reward
    {
        public List<int> AvatarIds { get; set; } = new List<int>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvatarIds.Count < 1 || AvatarIds.Count > 10)
                yield return new ValidationResult(
                    "AvatarIds must contain at least 1 element and at most 10",
                    new[] { nameof(AvatarIds) });
        }
    }

    [MessageSerde("AvatarReward")]
    public class AvatarRewardMessage : RewardMessage<AvatarReward>
    {
    }
}
