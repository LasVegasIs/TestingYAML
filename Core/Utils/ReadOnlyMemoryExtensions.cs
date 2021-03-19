using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crey.Utils
{
    public static class ReadOnlyMemoryExtensions
    {
        public static MemoryStream IntoMemoryStream(this ReadOnlyMemory<byte> span)
        {
            return new MemoryStream(span.ToArray(), 0, span.Length);
        }

        public static StreamReader IntoStreamReader(this ReadOnlyMemory<byte> span)
        {
            return new StreamReader(span.IntoMemoryStream());
        }

        public static JObject IntoJObject(this ReadOnlyMemory<byte> input)
        {
            var mem = new MemoryStream(input.ToArray(), 0, input.Length, false);
            var intputStream = new StreamReader(mem);
            var reader = new JsonTextReader(intputStream) { FloatParseHandling = FloatParseHandling.Decimal };
            var content = JObject.Load(reader);
            return content;
        }

        public static async Task<ReadOnlyMemory<byte>> IntoReadOnlyMemoryAsync(this Stream stream)
        {
            ReadOnlyMemory<byte> dataSpan = null;

            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                dataSpan = new ReadOnlyMemory<byte>(ms.ToArray());
            }

            return dataSpan;
        }
    }
}