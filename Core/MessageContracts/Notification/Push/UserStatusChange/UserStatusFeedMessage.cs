using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Notification.Push
{
    [JsonConverter(typeof(JsonMessageSerdeConverter<ActivityFeedMessage>))]
    public abstract class UserStatusFeedMessage : IPushNotification
    {
        public string Category => "UserStatusChange";
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        internal UserStatusFeedMessage() { }
    }
}
