namespace Crey.MessageContracts.Social
{
    [MessageSerde("FriendAdded")]
    public sealed class FriendAddedMessage : SocialMessage
    {
        public int AccountId { get; set; }
        public int FriendAccountId { get; set; }
        public ulong FriendsCount { get; set; }

        public FriendAddedMessage()
        {
        }

        public FriendAddedMessage(int accountId, int friendAccountId, ulong friendsCount)
        {
            AccountId = accountId;
            FriendAccountId = friendAccountId;
            FriendsCount = friendsCount;
        }
    }
}