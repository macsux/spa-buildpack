using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SpaLauncher.SpringCloudBus.Util;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringJsonConverter : JsonConverter
    {
        private static JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new[] {new EpochMillisecondTimeConverter()}
        });
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
            var jObj = JObject.FromObject(value, _serializer);
            var typeName = value.GetType().Name;
            jObj.AddFirst(new JProperty("type", typeName));
            jObj.WriteTo(writer);
		
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            
            var jobj = JObject.Load(reader);
            var typeName = jobj["type"].Value<string>();
            var type = this.GetType().Assembly
                .GetExportedTypes()
                .FirstOrDefault(x => x.Name == typeName);
            return jobj.ToObject(type, _serializer);
		
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => true;
    }
}