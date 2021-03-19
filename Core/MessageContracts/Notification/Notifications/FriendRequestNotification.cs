namespace Crey.MessageContracts.Notification
{
    [MessageSerde("FriendRequest")]
    public sealed class FriendRequestNotification : NotificationPayload
    {
        public int AccountId { get; set; }
    }

    [MessageSerde("FriendRequest")]
    public sealed class FriendRequestNotificationMessage : NotificationMessage<FriendRequestNotification>
    {
    }
}
