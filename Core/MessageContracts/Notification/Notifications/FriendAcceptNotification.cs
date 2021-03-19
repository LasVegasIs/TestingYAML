namespace Crey.MessageContracts.Notification
{
    [MessageSerde("FriendAccept")]
    public sealed class FriendAcceptNotification : NotificationPayload
    {
        public int AccountId { get; set; }
    }


    [MessageSerde("FriendAccept")]
    public sealed class FriendAcceptNotificationMessage : NotificationMessage<FriendAcceptNotification>
    {
    }
}
