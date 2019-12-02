using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Security;

namespace SpaLauncher.Actuators
{
    public class BusRefreshEndpointOptions : AbstractEndpointOptions, IBusRefreshEndpointOptions
    {
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints:bus-refresh";

        public BusRefreshEndpointOptions()
        {
            Id = "bus-refresh";
            RequiredPermissions = Permissions.NONE;
            
        }

        public BusRefreshEndpointOptions(IConfiguration config) : base(MANAGEMENT_INFO_PREFIX, config)
        {
            Id = "bus-refresh";
        }
    }
}