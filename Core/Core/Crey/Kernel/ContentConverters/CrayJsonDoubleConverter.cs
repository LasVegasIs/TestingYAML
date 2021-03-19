using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Crey.Kernel.ContentConverter
{
    internal class CrayJsonDoubleConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(float) || objectType == typeof(double);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = Convert.ToDecimal(value);
            if (Math.Abs(val) < 0.00000001m) val = 0.0m;

            // at least 1 decimal place and go up to 28 if needed 
            // never use scientific notation - this will cause some diffs in the json compared to the output of the CPP library
            var valueString = val.ToString("0.0############################", CultureInfo.InvariantCulture);
            writer.WriteRawValue(valueString);
        }
    }
}