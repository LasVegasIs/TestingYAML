namespace Crey.MessageContracts.Notification
{
    [MessageSerde("Description")]
    public sealed class DescriptionTrigger : RewardTrigger
    {
        public string Description { get; set; }

        internal DescriptionTrigger() { }

        public DescriptionTrigger(string description)
        {
            Description = description;
        }
    }
}