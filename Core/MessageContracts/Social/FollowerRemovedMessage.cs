namespace Crey.MessageContracts.Social
{
    [MessageSerde("FollowerRemoved")]
    public sealed class FollowerRemovedMessage : SocialMessage
    {
        public int AccountId { get; set; }
        public int FollowerAccountId { get; set; }
        public int FollowerCount { get; set; }

        public FollowerRemovedMessage()
        {
        }

        public FollowerRemovedMessage(int accountId, int followerAccountId, int followerCount)
        {
            AccountId = accountId;
            FollowerAccountId = followerAccountId;
            FollowerCount = followerCount;
        }
    }
}

