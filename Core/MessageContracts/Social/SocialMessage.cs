using MessageContracts.Utils;
using Newtonsoft.Json;
using System;

namespace Crey.MessageContracts.Social
{
    [MessageTopic("social")]
    [JsonConverter(typeof(JsonMessageSerdeConverter<SocialMessage>))]
    public abstract class SocialMessage : IMessageContract
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);
        internal SocialMessage() { }
    }
}
