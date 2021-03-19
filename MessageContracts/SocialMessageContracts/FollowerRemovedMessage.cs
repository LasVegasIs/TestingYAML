namespace SocialMessageContracts
{
    public class FollowerRemovedMessage : ISocialMessage
    {
        public string Type => "FollowerRemoved";

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

