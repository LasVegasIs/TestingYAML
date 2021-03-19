using Crey.MessageContracts.Rewards;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.GameEvents
{
    [MessageSerde("EventPointsReward")]
    public sealed class EventPointsReward : Reward
    {
        public long EventId { get; set; }
        public uint Amount { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount < 1)
                yield return new ValidationResult(
                    "Amount exceeds limits",
                    new[] { nameof(Amount) });

            if (EventId < 1)
                yield return new ValidationResult(
                    "Invalid event id",
                    new[] { nameof(Amount) });
        }
    }

    [MessageSerde("EventPointsReward")]
    public class EventPointsRewardMessage : RewardMessage<EventPointsReward> {}
}
