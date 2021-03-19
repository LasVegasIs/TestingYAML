namespace Crey.MessageContracts.Notification
{
    [MessageSerde("ThumbnailVote")]
    public sealed class ThumbnailVoteNotification : NotificationPayload
    {
        public int LevelId { get; set; }
        public int AccountId { get; set; }
        public int Count { get; set; }
    }

    [MessageSerde("ThumbnailVote")]
    public sealed class ThumbnailVoteNotificationMessage : NotificationMessage<ThumbnailVoteNotification>
    {        
    }
}
