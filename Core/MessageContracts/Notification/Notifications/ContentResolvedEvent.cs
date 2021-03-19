namespace Crey.MessageContracts.Notification
{

    [MessageSerde("ContentResolvedEvent")]
    public sealed class ContentResolvedEvent : NotificationPayload
    {
        public int Moderator { get; set; }
        public int ModeratedUser { get; set; }
        public string UserContent { get; set; }
        public string ButtonText { get; set; } = "Got it!";
        public ModeratedContentTypes Content { get; set; }
    }

    [MessageSerde("ContentResolvedEvent")]
    public sealed class ContentResolvedEventMessage : NotificationMessage<ContentResolvedEvent>
    {
    }
}
