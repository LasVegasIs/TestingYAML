namespace Crey.MessageContracts.Notification
{
    [MessageSerde("LevelLike")]
    public sealed class LevelLikeNotification : NotificationPayload
    {
        public int LevelId { get; set; }
        public int AccountId { get; set; }
        public int Count { get; set; }
    }

    [MessageSerde("LevelLike")]
    public sealed class LevelLikeNotificationMessage : NotificationMessage<LevelLikeNotification>
    {
    }
}
