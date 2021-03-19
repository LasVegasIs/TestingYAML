using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Crey.MessageStream.REST
{
    public static class MessageFactoryRESTExtension
    {
        [Obsolete]
        public static TMessageType Deserialize<TMessageType>(this IMessageFactory<TMessageType> factory, string typeName, HttpRequest request)
            where TMessageType : IRESTMessage
        {
            Stream req = request.Body;
            string json = new StreamReader(req).ReadToEnd();
            var payload = JObject.Parse(json);
            factory.TypeMap.TryGetValue(typeName, out Type type);
            return (TMessageType)payload.ToObject(type);
        }


        public static async Task<TMessageType> DeserializeAsync<TMessageType>(this IMessageFactory<TMessageType> factory, string typeName, HttpRequest request)
    where TMessageType : IRESTMessage
        {
            Stream req = request.Body;
            string json = await new StreamReader(req).ReadToEndAsync();
            var payload = JObject.Parse(json);
            factory.TypeMap.TryGetValue(typeName, out Type type);
            return (TMessageType)payload.ToObject(type);
        }

    }
}
