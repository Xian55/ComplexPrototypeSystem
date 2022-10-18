using System.Net;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ComplexPrototypeSystem.Shared.Converters
{
    public class IPAddressConverter : JsonConverter<IPAddress>
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPAddress));
        }

        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return IPAddress.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
