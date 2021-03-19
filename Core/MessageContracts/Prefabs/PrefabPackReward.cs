using Crey.MessageContracts.Rewards;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.Prefabs
{
    [MessageSerde("PrefabPackReward")]
    public sealed class PrefabPackReward : Reward
    {
        public List<int> PackIds { get; set; } = new List<int>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PackIds.Count < 1 || PackIds.Count > 10)
                yield return new ValidationResult(
                    "PackIds must contain at least 1 element and at most 10",
                    new[] { nameof(PackIds) });
        }
    }

    [MessageSerde("PrefabPackReward")]
    public sealed class PrefabPackRewardMessage : RewardMessage<PrefabPackReward>
    {
    }
}