using Crey.MessageContracts.Rewards;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.UserProfile
{
    [MessageSerde("XPReward")]
    public sealed class UserXPReward : Reward
    {
        public uint Amount { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount < 1 || Amount > 10000)
                yield return new ValidationResult(
                    "Amount exceeds limits",
                    new[] { nameof(Amount) });
        }
    }

    [MessageSerde("XPReward")]
    public sealed class UserXPRewardMessage : RewardMessage<UserXPReward>
    {
    }
}
