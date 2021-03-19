using MessageContracts.Utils;
using Newtonsoft.Json;

namespace Crey.MessageContracts.Notification
{
    [JsonConverter(typeof(JsonMessageSerdeConverter<RewardTrigger>))]
    public abstract class RewardTrigger
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);
        internal RewardTrigger() { }
    }
}