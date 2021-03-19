namespace Crey.MessageContracts.Notification
{
    [MessageSerde("GivenBySupportTrigger")]
    public sealed class GivenBySupportTrigger : RewardTrigger
    {
        public string Description { get; set; }
        public int AccountId { get; set; }

        internal GivenBySupportTrigger() { }

        public GivenBySupportTrigger(int accountId, string description)
        {
            Description = description;
            AccountId = accountId;
        }
    }
}