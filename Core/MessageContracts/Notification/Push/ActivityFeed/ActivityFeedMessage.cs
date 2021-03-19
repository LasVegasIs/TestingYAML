using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Notification.Push
{
    [JsonConverter(typeof(JsonMessageSerdeConverter<ActivityFeedMessage>))]
    public abstract class ActivityFeedMessage : IPushNotification
    {
        public string Category => "ActivityFeed";
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        internal ActivityFeedMessage() { }
    }

    // For future versions, derive from ActivityFeedMessage directly but it's a breaking change in the ui
    // as the "Payload" would be flattened
    public abstract class DeprecatedActivityFeedMessage<T> : ActivityFeedMessage
        where T : new()
    {
        public T Payload { get; private set; }

        internal DeprecatedActivityFeedMessage() { Payload = new T(); }
        internal DeprecatedActivityFeedMessage(T payload) { Payload = payload; }
    }
}
