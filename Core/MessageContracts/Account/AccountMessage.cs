using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Account
{
    [MessageTopic("user-account")]
    [JsonConverter(typeof(JsonMessageSerdeConverter<AccountMessage>))]
    public abstract class AccountMessage
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        public int AccountId { get; set; }

        internal AccountMessage() { }
    }
}
