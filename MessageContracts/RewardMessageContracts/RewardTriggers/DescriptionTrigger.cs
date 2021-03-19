namespace MessageContracts.RewardTriggers
{
    public class DescriptionTrigger: IRewardTrigger
    {
        public const string TriggerType = "Description";
        public string Type => TriggerType;

        public string Description { get; set; }

        public DescriptionTrigger(string description)
        {
            Description = description;
        }
    }
}