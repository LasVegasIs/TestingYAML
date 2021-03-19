namespace SocialMessageContracts
{
    public class FriendRemoved : ISocialMessage
    {
        public string Type => "FriendRemoved";

        public int AccountId { get; set; }
        public int FriendAccountId { get; set; }
        public ulong FriendsCount { get; set; }

        public FriendRemoved()
        {
        }

        public FriendRemoved(int accountId, int friendAccountId, ulong friendsCount)
        {
            AccountId = accountId;
            FriendAccountId = friendAccountId;
            FriendsCount = friendsCount;
        }
    }
}