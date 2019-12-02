using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace SpaLauncher
{
    public class AppConfig
    {
        
        private readonly IConfiguration _configuration;

        public AppConfig(IConfiguration configuration)
        {

            var root = (IConfigurationRoot) configuration;
            var configServerProvider = root.Providers.OfType<ConfigServerConfigurationProvider>().Cast<IConfigurationProvider>().ToList();
            _configuration = new ConfigurationRoot(configServerProvider);
        }

        public string GetConfigJson()
        {

            return RemoveEmptyChildren(Serialize(_configuration)).ToString(Formatting.None);
        }
        private JToken Serialize(IConfiguration config)
        {
            JObject obj = new JObject();
            foreach (var child in config.GetChildren())
            {
                if (child.Path.StartsWith("spring:cloud:config", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
                return new JValue(InferType(section.Value));

            return obj;
        }

        private object InferType(string value)
        {
            if(value == null) 
                return value;
            if(int.TryParse(value, out var i))
                return i;
            if(decimal.TryParse(value, out var d))
                return d;
            if(bool.TryParse(value, out var b))
                return b;
            return value;
        }
        
        private static JToken RemoveEmptyChildren(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        private static bool IsEmpty(JToken token)
        {
            return (token.Type == JTokenType.Null || (!token.HasValues && token.Type == JTokenType.Object));
        }
    }
}