namespace Crey.MessageContracts.Notification
{
    [MessageSerde("FollowUser")]
    public sealed class FollowUserNotification : NotificationPayload
    {
        public int FollowingUserId { get; set; }
    }

    [MessageSerde("FollowUser")]
    public sealed class FollowUserNotificationMessage : NotificationMessage<FollowUserNotification>
    {
    }
}
