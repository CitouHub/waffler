using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Waffler.Service.Converter
{
    public class DecimalStringFormatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var decimalValue = ((decimal)value).ToString("N8", CultureInfo.InvariantCulture);
            var formatedDecimalValue = decimalValue.Replace(",", "").TrimEnd('0');
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