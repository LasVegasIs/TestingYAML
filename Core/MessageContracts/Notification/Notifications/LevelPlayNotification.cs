namespace Crey.MessageContracts.Notification
{
    [MessageSerde("LevelPlay")]
    public sealed class LevelPlayNotification : NotificationPayload
    {
        public int LevelId { get; set; }
        public ulong Count { get; set; }
    }

    [MessageSerde("LevelPlay")]
    public sealed class LevelPlayNotificationMessage : NotificationMessage<LevelPlayNotification>
    {
    }
}
