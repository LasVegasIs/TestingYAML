namespace Crey.MessageContracts.Notification
{
    [MessageSerde("LevelComment")]
    public sealed class LevelCommentNotification : NotificationPayload
    {
        public int LevelId { get; set; }
        public CommentId CommentId { get; set; }
        public int AccountId { get; set; }
    }

    [MessageSerde("LevelComment")]
    public sealed class LevelCommentNotificationMessage : NotificationMessage<LevelCommentNotification>
    {
    }
}
