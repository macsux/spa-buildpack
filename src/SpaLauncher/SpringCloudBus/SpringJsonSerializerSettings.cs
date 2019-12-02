using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringJsonSerializerSettings
    {
        static SpringJsonSerializerSettings()
        {
            Instance = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[]
                {
                    new SpringJsonConverter()
                }
            };
        }

        public static JsonSerializerSettings Instance { get; }
    }
}