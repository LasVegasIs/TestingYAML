using System.ComponentModel.DataAnnotations;

namespace MessageContracts.Rewards
{
    public enum Currency
    {
        RealMoney_EUR,
        Gold,
        KarmaPoint,
        KarmaCoin,
    }

    public class MoneyReward : IReward, IRewardDetail
    {
        public const string RewardType = "MoneyReward";
        public string Type => RewardType;

        [Range(1, 1000, ErrorMessage = "Amount exceeds limits(1..1000)")]
        public uint Amount { get; set; }

        public Currency Currency { get; set; }
    }

    public class MoneyRewardMessage : RewardMessage<MoneyReward> { }
}