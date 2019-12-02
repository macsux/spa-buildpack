using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SpaLauncher.SpringCloudBus.Util;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringNodeNameResoler : INodeNameResoler
    {
        private readonly IConfiguration _configuration;
        private readonly ServerPortAccessor _portAccessor;
        private SpringPathMatcher _springPathMatcher = new SpringPathMatcher();

        public SpringNodeNameResoler(IConfiguration configuration, ServerPortAccessor portAccessor)
        {
            _configuration = configuration;
            _portAccessor = portAccessor;
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
        public string InstanceId => _configuration.GetValue<string>("vcap.application.instance_id") ?? Guid.NewGuid().ToString("N");

        public string ServiceId => $"{AppName}.{Index}.{InstanceId}";

        public bool AddressedToMe(string destination)
        {
            return _springPathMatcher.IsMatch(destination, ServiceId.Replace(".",":"));
        }
    }
}