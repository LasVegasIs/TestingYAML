using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.UserProfile
{
    [MessageTopic("user-profile")]
    [JsonConverter(typeof(JsonMessageSerdeConverter<UserProfileMessage>))]
    public abstract class UserProfileMessage : IMessageContract
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);
        public int AccountId { get; set; }

        internal UserProfileMessage() { }
    }
}
