namespace SocialMessageContracts
{
    public class FollowerAddedMessage : ISocialMessage
    {
        public string Type => "FollowerAdded";

        public int AccountId { get; set; }
        public int FollowerAccountId { get; set; }
        public int FollowerCount { get; set; }

        public FollowerAddedMessage()
        {
        }

        public FollowerAddedMessage(int accountId, int followerAccountId, int followerCount)
        {
            AccountId = accountId;
            FollowerAccountId = followerAccountId;
            FollowerCount = followerCount;
        }
    }
}

