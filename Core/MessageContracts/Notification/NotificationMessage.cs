using MessageContracts.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Crey.MessageContracts.Notification
{
    [MessageTopic("notification")]
    [JsonConverter(typeof(JsonMessageSerdeConverter<NotificationMessage>))]
    public abstract class NotificationMessage : IMessageContract
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);
        public int TargetAccountId { get; set; }
        public NotificationType NotificationType { get; set; }

        public abstract NotificationPayload GetPayload();
    }

    public abstract class NotificationMessage<TNotification> : NotificationMessage
        where TNotification : NotificationPayload
    {
        public TNotification Notification { get; set; }
        public override NotificationPayload GetPayload() { return Notification; }
    }
}