using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core.Utilities
{
    public class ISO8601DateConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Date)
                throw new FormatException($"Unexpected token parsing date. Expected date or string type, got {reader.TokenType}");

            return DateTime.Parse(Convert.ToString(reader.Value));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                DateTime dValue = Convert.ToDateTime(value);
                string sDate = dValue.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                writer.WriteValue(sDate);
            }
            else
            {
                throw new FormatException("Can only be used on DateTime objects");
            }
        }
    }
}
