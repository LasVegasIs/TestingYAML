namespace Crey.MessageContracts.Social
{
    [MessageSerde("StartFollowUser")]
    public sealed class StartFollowUserMessage : SocialMessage
    {
        public int AccountId { get; set; }
        public int FollowedAccountId { get; set; }
        public int FollowedUserCount { get; set; }

        public StartFollowUserMessage()
        {
        }

        public StartFollowUserMessage(int accountId, int followedAccountId, int followedUserCount)
        {
            AccountId = accountId;
            FollowedAccountId = followedAccountId;
            FollowedUserCount = followedUserCount;
        }
    }
}
