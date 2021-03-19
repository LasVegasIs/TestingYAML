using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crey.MessageStream
{
    public interface IMessageFactory<TMessageType>
        where TMessageType : IStreamedMessage
    {
        DateTime CurrentVersion { get; }

        Dictionary<string, Type> TypeMap { get; }
    }

    public abstract class MessageFactory<T> : IMessageFactory<T>
        where T : IStreamedMessage
    {
        public abstract DateTime CurrentVersion { get; }
        public Dictionary<string, Type> TypeMap { get; private set; }

        protected MessageFactory(Dictionary<string, Type> typeMap)
        {
            TypeMap = typeMap;
        }

        protected static KeyValuePair<string, Type> Register<T2>()
            where T2 : T, new()
        {
            return KeyValuePair.Create(new T2().Type, typeof(T2));
        }
    }

    public static class IMessageFactoryExtension
    {
        private static (String, Type, JObject) ParseMessage<TMessageType>(this IMessageFactory<TMessageType> factory, JObject json)
            where TMessageType : IStreamedMessage
        {
            var typeName = json.GetValue("Type").ToString();
            factory.TypeMap.TryGetValue(typeName, out Type type);
            var payload = json.GetValue("Payload") as JObject;
            if (payload != null)
                return (typeName, type, payload);
            else
                return (typeName, type, json);
        }

        public static TMessageType Deserialize<TMessageType>(this IMessageFactory<TMessageType> factory, byte[] json)
            where TMessageType : IStreamedMessage
        {
            return factory.Deserialize(Encoding.UTF8.GetString(json));
        }

        public static TMessageType Deserialize<TMessageType>(this IMessageFactory<TMessageType> factory, string json)
            where TMessageType : IStreamedMessage
        {
            return factory.Deserialize(JObject.Parse(json));
        }

        public static TMessageType Deserialize<TMessageType>(this IMessageFactory<TMessageType> factory, JObject json)
            where TMessageType : IStreamedMessage
        {
            var (typeName, type, payload) = factory.ParseMessage(json);
            return (TMessageType)payload.ToObject(type);
        }

        public static (string, TMessageType) DeserializeWithTypeName<TMessageType>(this IMessageFactory<TMessageType> factory, JObject json)
            where TMessageType : IStreamedMessage
        {
            var (typeName, type, payload) = factory.ParseMessage(json);
            return (typeName, (TMessageType)payload.ToObject(type));
        }

        public static TMessageType Deserialize<TMessageType>(this IMessageFactory<TMessageType> factory, string typeName, string json)
            where TMessageType : IStreamedMessage
        {
            factory.TypeMap.TryGetValue(typeName, out Type type);
            return (TMessageType)JsonConvert.DeserializeObject(json, type);
        }
    }
}
