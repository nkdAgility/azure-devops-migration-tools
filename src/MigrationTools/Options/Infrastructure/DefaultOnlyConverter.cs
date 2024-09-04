using Newtonsoft.Json;
using System;

namespace MigrationTools.Options.Infrastructure
{


    public class DefaultOnlyConverter<T> : JsonConverter
    {
        private readonly T _defaultValue;

        public DefaultOnlyConverter(T defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Always write the default value, no matter what the actual value is
            writer.WriteValue(_defaultValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Allow deserialization to work as normal
            return serializer.Deserialize(reader, objectType);
        }
    }


}