namespace Crey.MessageContracts.Notification.Push
{
    /// <summary>
    /// Wrap a notification into a feed message.
    /// Other option would be to "derive" and use generic but it seemed to be more reusable
    /// </summary>
    public sealed class NotificationFeedMessage : IPushNotification
    {
        public string Category => "Notification";
        public NotificationPayload Notification { get; set; }
    }
}
