using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using MessagingCore;

namespace MessageContracts.RewardTriggers
{
    public interface IRewardTrigger
    {
        public string Type { get; }
    }

    public class RewardTriggerConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IRewardTrigger));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var rewardType = jo["Type"]?.Value<string>();
            switch (rewardType)
            {
                case DescriptionTrigger.TriggerType: return jo.ToObject<DescriptionTrigger>(serializer);
                default: throw new ContractViolationException($"Unknown IRewardTrigger type ({rewardType})");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}