﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;

namespace Crey.MessageContracts.Utils
{
    public static class JsonExtensions
    {
        public static JToken DefaultFromObject(this JsonSerializer serializer, object value)
        {
            if (value == null)
                return JValue.CreateNull();
            var dto = Activator.CreateInstance(typeof(DefaultSerializationDTO<>).MakeGenericType(value.GetType()), value);
            var root = JObject.FromObject(dto, serializer);
            return root["Value"].RemoveFromLowestPossibleParent() ?? JValue.CreateNull();
        }

        public static object DefaultToObject(this JToken token, Type type, JsonSerializer serializer = null)
        {
            var oldParent = token.Parent;

            var dtoToken = new JObject(new JProperty("Value", token));
            var dtoType = typeof(DefaultSerializationDTO<>).MakeGenericType(type);
            var dto = (IHasValue)(serializer ?? JsonSerializer.CreateDefault()).Deserialize(dtoToken.CreateReader(), dtoType);

            if (oldParent == null)
                token.RemoveFromLowestPossibleParent();

            return dto == null ? null : dto.GetValue();
        }

        public static JToken RemoveFromLowestPossibleParent(this JToken node)
        {
            if (node == null)
                return null;
            // If the parent is a JProperty, remove that instead of the token itself.
            var contained = node.Parent is JProperty ? node.Parent : node;
            contained.Remove();
            // Also detach the node from its immediate containing property -- Remove() does not do this even though it seems like it should
            if (contained is JProperty)
                ((JProperty)node.Parent).Value = null;
            return node;
        }

        /// <summary>
        /// Call the default serializer ignoring the class level converter attribute
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteDefaultJson(this JsonSerializer serializer, JsonWriter writer, object value)
        {
            JToken t = serializer.DefaultFromObject(value);
            t.WriteTo(writer);
        }
        
        interface IHasValue
        {
            object GetValue();
        }

        [JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy), IsReference = false)]
        class DefaultSerializationDTO<T> : IHasValue
        {
            public DefaultSerializationDTO(T value) { this.Value = value; }

            public DefaultSerializationDTO() { }

            [JsonConverter(typeof(NoConverter)), JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
            public T Value { get; set; }

            object IHasValue.GetValue() { return Value; }
        }
    }

    public class NoConverter : JsonConverter
    {
        // NoConverter taken from this answer https://stackoverflow.com/a/39739105/3744182
        // To https://stackoverflow.com/questions/39738714/selectively-use-default-json-converter
        // By https://stackoverflow.com/users/3744182/dbc
        public override bool CanConvert(Type objectType) { throw new NotImplementedException(); /* This converter should only be applied via attributes */ }

        public override bool CanRead { get { return false; } }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) { throw new NotImplementedException(); }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { throw new NotImplementedException(); }
    }
}
