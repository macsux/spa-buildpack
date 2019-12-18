using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace SpaLauncher
{
    public class AppConfig
    {
        private readonly IConfiguration _configuration;

        private readonly IConfiguration _safeConfiguration;

        public AppConfig(IConfiguration configuration)
        {
            _configuration = configuration;
            // we wanna create a new root with placeholder provider backed by only config server provider which we extract from application configuration
            // this is so we don't publish sensitive config found in other config sources
            var root = (IConfigurationRoot) configuration;
            var existingPlaceholder = root.Providers.OfType<PlaceholderResolverProvider>().First();
            
            var exposedProviders = existingPlaceholder.Providers
                .Where(x => x is ConfigServerConfigurationProvider)
                .ToList();
            var placeholderProvider = new PlaceholderResolverProvider(exposedProviders);
            _safeConfiguration = new ConfigurationRoot(new IConfigurationProvider[]{placeholderProvider});
        }

        public string GetConfigJson()
        {

            var config = (JObject)Serialize(_safeConfiguration);
            var sso = _configuration.GetSingletonServiceInfo<SsoServiceInfo>();
            if (sso != null)
            {
                config.Add("SSO", JObject.FromObject(new SsoServiceInfo(sso.Id, sso.ClientId, null, sso.AuthDomain)));
            }

            return RemoveEmptyChildren(config).ToString(Formatting.None);
                
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