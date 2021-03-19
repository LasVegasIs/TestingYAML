using Crey.MessageContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.MessageStream
{
#nullable enable
    public class MessageSerializer<TMessageType>
    {
        public static MessageSerializer<TMessageType> CreateDefault()
        {
            return new MessageSerializer<TMessageType>(new JsonSerializer());
        }


        public JsonSerializer Serializer { get; set; }

        public MessageSerializer(JsonSerializer serializer)
        {
            Serializer = serializer;
        }
    }

    public static class MessageSerializerExtension
    {
        public static string Serialize<T, TMessageType>(this MessageSerializer<TMessageType> serde, T message)
            where TMessageType : IMessageContract
        {
            var serializer = serde.Serializer;

            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = serializer.Formatting;
                serializer.Serialize(jsonWriter, message);
            }

            return sw.ToString();
        }

        public static JObject? DeserializeJObject<TMessageType>(this MessageSerializer<TMessageType> serde, string jsonStr)
            where TMessageType : IMessageContract
        {
            using (var reader = new JsonTextReader(new StringReader(jsonStr)))
            {
                return serde.Serializer.Deserialize<JObject>(reader);
            }
        }

        public static JObject? DeserializeJObject<TMessageType>(this MessageSerializer<TMessageType> serde, JsonReader reader)
            where TMessageType : IMessageContract
        {
            return serde.Serializer.Deserialize<JObject>(reader);
        }

        public static TMessageType? DeserializeMessage<TMessageType>(this MessageSerializer<TMessageType> serde, string jsonStr)
            where TMessageType : IMessageContract
        {
            using (var reader = new JsonTextReader(new StringReader(jsonStr)))
            {
                return serde.Serializer.Deserialize<TMessageType>(reader);
            }
        }

        public static TMessageType? DeserializeMessage<TMessageType>(this MessageSerializer<TMessageType> serde, JsonReader reader)
            where TMessageType : IMessageContract
        {
            return serde.Serializer.Deserialize<TMessageType>(reader);
        }

        public static TMessageType? DeserializeMessage<TMessageType>(this MessageSerializer<TMessageType> serde, JObject jobject)
            where TMessageType : IMessageContract
        {
            return jobject.ToObject<TMessageType>(serde.Serializer);
        }

        public static async Task<TMessageType?> DeserializeMessage<TMessageType>(this MessageSerializer<TMessageType> serde, HttpRequest request)
            where TMessageType : IMessageContract
        {
            using var req = request.Body;
            string json = await new StreamReader(req).ReadToEndAsync();
            return serde.DeserializeMessage(json);
        }

        public static IServiceCollection TryAddMessageSerializer<TMessageType>(this IServiceCollection collectionBuilder, MessageSerializer<TMessageType> serializer)
            where TMessageType : class, IMessageContract
        {
            if (!collectionBuilder.HasMessageSerializer<TMessageType>())
                collectionBuilder.AddSingleton(serializer);
            return collectionBuilder;
        }

        public static IServiceCollection TryAddMessageSerializer<TMessageType>(this IServiceCollection collectionBuilder)
            where TMessageType : class, IMessageContract
        {
            return collectionBuilder.TryAddMessageSerializer(MessageSerializer<TMessageType>.CreateDefault());
        }

        public static bool HasMessageSerializer<TMessageType>(this IServiceCollection collectionBuilder)
            where TMessageType : class, IMessageContract
        {
            return collectionBuilder.Any(d => d.ServiceType == typeof(MessageSerializer<TMessageType>));
        }

        public static IServiceCollection AssertMessageSerializer<TMessageType>(this IServiceCollection collectionBuilder)
            where TMessageType : class, IMessageContract
        {
            if (!collectionBuilder.HasMessageSerializer<TMessageType>())
                throw new InternalErrorException($"Message serializer was not registered: {typeof(TMessageType).Name}");
            return collectionBuilder;
        }
    }
}