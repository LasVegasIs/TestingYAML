namespace UserProfileMessageContracts
{
    public class UserXPLevelRewardTriggerMessage : UserProfileMessage
    {
        public override string Type => "UserXPLevelRewardTrigger";
        public string RewardId { get; set; }
        public uint Level { get; set; }
    }
}