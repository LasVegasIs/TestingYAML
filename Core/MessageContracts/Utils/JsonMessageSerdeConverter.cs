using Crey.MessageContracts;
using Crey.MessageContracts.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MessageContracts.Utils
{
    public class JsonMessageSerdeConverter<TMessageBase> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TMessageBase));
        }

        private class TypeInfo
        {
            public string Type { get; set; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject jo = JObject.Load(reader);
            var typeInfo = jo.ToObject<TypeInfo>(serializer);
            if (typeInfo.Type == null)
                return null; // something unexpected

            var type = MessageSerdeAttribute.GetTypeForSerdeToken<TMessageBase>(typeInfo.Type)
                ?? throw new ContractViolationException($"Unknown type ({typeInfo.Type})");

            existingValue = (TMessageBase)Activator.CreateInstance(type);

            using var sr = jo.CreateReader();
            serializer.Populate(sr, existingValue);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.WriteDefaultJson(writer, value);
        }
    }
}
