using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Content
{
    [MessageTopic("content")]
    [JsonConverter(typeof(JsonMessageSerdeConverter<ContentMessage>))]
    public abstract class ContentMessage : IMessageContract
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        internal ContentMessage() { }

    }
}
