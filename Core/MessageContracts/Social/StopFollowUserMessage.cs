namespace Crey.MessageContracts.Social
{
    [MessageSerde("StopFollowUser")]
    public sealed class StopFollowUserMessage : SocialMessage
    {
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
