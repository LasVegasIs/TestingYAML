using Microsoft.Azure.KeyVault;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Crey.Utils
{
    [Obsolete("Use ReadOnlyMemory<byte>")]
    public class DataSpan
    {
        public byte[] Buffer;
        public int Start;
        public int Length;

        public DataSpan(byte[] buffer)
        {
            Buffer = buffer;
            Start = 0;
            Length = Buffer.Length;
        }

        public DataSpan(byte[] buffer, int start, int index)
        {
            Buffer = buffer;
            Start = start;
            Length = index;
        }

        public bool IsFull => Start == 0 && Length == Buffer.Length;
        public bool IsPartial => !IsFull;
        public byte[] FullBufferRef => (Buffer != null && IsPartial) ? CopyArray() : Buffer;

        public byte[] CopyArray()
        {
            return Buffer.Skip(Start).Take(Length).ToArray();
        }

        public static implicit operator ReadOnlyMemory<byte>(DataSpan self) => new ReadOnlyMemory<byte>(self.Buffer, self.Start, self.Length);
    };

    public static class DataSpanExtensions
    {
        public static MemoryStream IntoMemoryStream(this DataSpan span)
        {
            return new MemoryStream(span.Buffer, span.Start, span.Length);
        }

        public static StreamReader IntoStreamReader(this DataSpan span)
        {
            return new StreamReader(span.IntoMemoryStream());
        }

        public static bool IsNullOrEmpty(this DataSpan span)
        {
            return span == null || span.Length == 0;
        }

        public static JObject IntoJObject(this DataSpan input)
        {
            var mem = new MemoryStream(input.Buffer, input.Start, input.Length, false);
            var intputStream = new StreamReader(mem);
            var reader = new JsonTextReader(intputStream) { FloatParseHandling = FloatParseHandling.Decimal };
            var content = JObject.Load(reader);
            return content;
        }

        public static async Task<DataSpan> IntoDataSpanAsync(this Stream stream)
        {
            DataSpan dataSpan = null;

            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                dataSpan = new DataSpan(ms.ToArray());
            }

            return dataSpan;
        }
    }
}
