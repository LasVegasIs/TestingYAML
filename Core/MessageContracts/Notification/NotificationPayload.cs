using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Notification
{
    [JsonConverter(typeof(JsonMessageSerdeConverter<NotificationPayload>))]
    public abstract class NotificationPayload : IMessageContract
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        internal NotificationPayload() { }
    }
}

