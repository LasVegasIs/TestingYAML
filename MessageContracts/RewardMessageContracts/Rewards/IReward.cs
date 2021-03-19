using MessagingCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MessageContracts.Rewards
{
    public interface IReward
    {
        public string Type { get; }
    }

    public class RewardConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IReward));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var rewardType = jo["Type"]?.Value<string>();

            switch (rewardType)
            {
                case AvatarReward.RewardType: return jo.ToObject<AvatarReward>(serializer);
                case MoneyReward.RewardType: return jo.ToObject<MoneyReward>(serializer);
                case XPReward.RewardType: return jo.ToObject<XPReward>(serializer);
                case EventPointsReward.RewardType: return jo.ToObject<EventPointsReward>(serializer);
                case PrefabPackReward.RewardType: return jo.ToObject<PrefabPackReward>(serializer);
                case NoReward.RewardType: return jo.ToObject<NoReward>(serializer);
                default: throw new ContractViolationException($"Unknown IReward type ({rewardType})");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    
}