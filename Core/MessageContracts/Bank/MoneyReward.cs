using Crey.MessageContracts.Rewards;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.Bank
{
    [MessageSerde("MoneyReward")]
    public sealed class MoneyReward : Reward
    {
        public uint Amount { get; set; }
        public Currency Currency { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount < 1 || Amount > 1000)
                yield return new ValidationResult(
                    "Amount exceeds limits",
                    new[] { nameof(Amount) });
        }
    }

    [MessageSerde("MoneyReward")]
    public class MoneyRewardMessage : RewardMessage<MoneyReward> { }
}