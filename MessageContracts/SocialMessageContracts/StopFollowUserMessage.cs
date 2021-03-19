namespace SocialMessageContracts
{
    public class StopFollowUserMessage : ISocialMessage
    {
        public string Type => "StopFollowUser";

        public int AccountId { get; set; }
        public int FollowedAccountId { get; set; }
        public int FollowedUserCount { get; set; }

        public StopFollowUserMessage()
        {
        }

        public StopFollowUserMessage(int accountId, int followedAccountId, int followedUserCount)
        {
            AccountId = accountId;
            FollowedAccountId = followedAccountId;
            FollowedUserCount = followedUserCount;
        }
    }
}
