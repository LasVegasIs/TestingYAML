namespace SocialMessageContracts
{
    public class FriendAdded : ISocialMessage
    {
        public string Type => "FriendAdded";

        public int AccountId { get; set; }
        public int FriendAccountId { get; set; }
        public ulong FriendsCount { get; set; }

        public FriendAdded()
        {
        }

        public FriendAdded(int accountId, int friendAccountId, ulong friendsCount)
        {
            AccountId = accountId;
            FriendAccountId = friendAccountId;
            FriendsCount = friendsCount;
        }
    }
}