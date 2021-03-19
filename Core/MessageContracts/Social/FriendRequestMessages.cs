namespace Crey.MessageContracts.Social
{
    public abstract class FriendMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int RequestFriendUserId { get; set; }
    }

    [MessageSerde("FriendRequest")]
    public sealed class FriendRequestMessage : FriendMessage
    {
    }

    [MessageSerde("AcceptRequest")]
    public sealed class FriendAcceptRequestMessage : FriendMessage
    {
    }

    [MessageSerde("RejectRequest")]
    public sealed class RejectRequestMessage : FriendMessage
    {
    }

    [MessageSerde("DeleteRequest")]
    public sealed class DeleteRequestMessage : FriendMessage
    {
    }
}