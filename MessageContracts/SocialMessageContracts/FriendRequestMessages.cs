namespace SocialMessageContracts
{
    public abstract class FriendMessage : SocialMessage
    {
        public int RequestFriendUserId { get; set; }

        public FriendMessage(string type)
            : base(type)
        {
        }
    }

    public class FriendRequestMessage : FriendMessage
    {

        public FriendRequestMessage()
            : base("FriendRequest")
        {
        }
    }

    public class AcceptRequestMessage : FriendMessage
    {

        public AcceptRequestMessage()
            : base("AcceptRequest")
        {
        }
    }

    public class RejectRequestMessage : FriendMessage
    {

        public RejectRequestMessage()
            : base("RejectRequest")
        {
        }
    }

    public class DeleteRequestMessage : FriendMessage
    {

        public DeleteRequestMessage()
            : base("DeleteRequest")
        {
        }
    }
}