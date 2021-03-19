namespace Crey.MessageContracts.Social
{
    [MessageSerde("FriendRemoved")]
    public sealed class FriendRemovedMessage : SocialMessage
    {
        public int AccountId { get; set; }
        public int FriendAccountId { get; set; }
        public ulong FriendsCount { get; set; }

        public FriendRemovedMessage()
        {
        }

        public FriendRemovedMessage(int accountId, int friendAccountId, ulong friendsCount)
        {
            AccountId = accountId;
            FriendAccountId = friendAccountId;
            FriendsCount = friendsCount;
        }
    }
}