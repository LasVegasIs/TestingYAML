namespace Crey.MessageContracts.Notification
{
    [MessageSerde("ContentRestrictedEvent")]
    public sealed class ContentRestrictedEvent : NotificationPayload
    {
        public int Moderator { get; set; }
        public int ModeratedUser { get; set; }
        public string UserContent { get; set; }
        public string ButtonText { get; set; } = "Got it!";
        public ModeratedContentTypes Content { get; set; }
    }

    [MessageSerde("ContentRestrictedEvent")]
    public sealed class ContentRestrictedEventMessage : NotificationMessage<ContentRestrictedEvent>
    {
    }
}
