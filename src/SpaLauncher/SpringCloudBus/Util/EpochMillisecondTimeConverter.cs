using System;
using Newtonsoft.Json;

namespace SpaLauncher.SpringCloudBus.Util
{
    public class EpochMillisecondTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTimeExtensions.FromEpochMillisececonds((long)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToEpochMillisececonds());
        }
    }
}