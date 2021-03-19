namespace Crey.MessageContracts.Rewards
{
    /// Reward that grants nothing. 
    [MessageSerde("NoReward")]
    public sealed class NoReward : Reward
    {
    }

    [MessageSerde("NoReward")]
    public sealed class NoRewardMessage : RewardMessage<NoReward> { }
}