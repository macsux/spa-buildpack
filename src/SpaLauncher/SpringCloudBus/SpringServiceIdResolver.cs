using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SpaLauncher.SpringCloudBus.Util;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringServiceIdResolver : IServiceIdResolver
    {
        private readonly IConfiguration _configuration;
        private readonly IServerPortAccessor _portAccessor;
        private SpringPathMatcher _springPathMatcher = new SpringPathMatcher();

        public SpringServiceIdResolver(IConfiguration configuration, IServerPortAccessor portAccessor)
        {
            _configuration = configuration;
            _portAccessor = portAccessor;
            _instanceId = new Lazy<string>(() => _configuration.GetValue<string>("vcap.application.instance_id") ?? Guid.NewGuid().ToString("N"));
        }


        public string AppName
        {
            get
            {
                var vcapName = _configuration.GetValue<string>("vcap:application:name");
                var springName = _configuration.GetValue<string>("spring:application:name");
                var name = vcapName ?? springName;
                return name;
            }
        }

        public int Index
        {
            get
            {
                var vcapIndex = _configuration.GetValue<int>("vcap.application.instance_index");
                var springIndex = _configuration.GetValue<int>("spring.application.index");
                var serverPort = _portAccessor.Port;
                var index = new[] {vcapIndex, springIndex, serverPort}.FirstOrDefault(x => x > 0);
                return index;
            }
        }

        private Lazy<string> _instanceId;
        public string InstanceId => _instanceId.Value;

        public string ServiceId => $"{AppName}.{Index}.{InstanceId}";

        public bool AddressedToMe(string destination)
        {
            return _springPathMatcher.IsMatch(destination, ServiceId.Replace(".",":"));
        }
    }
}