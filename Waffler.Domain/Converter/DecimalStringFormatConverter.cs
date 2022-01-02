using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Waffler.Domain.Converter
{
    public class DecimalStringFormatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || 
                objectType == typeof(double) ||
                objectType == typeof(float);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var decimalValue = Convert.ToDecimal(value).ToString("N8", CultureInfo.InvariantCulture);
            var formatedDecimalValue = decimalValue.Replace(",", "").TrimEnd('0').TrimEnd('.');
            writer.WriteValue(formatedDecimalValue);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}