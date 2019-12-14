using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SpaLauncher.SpringCloudBus.Util;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Hypermedia;

namespace SpaLauncher.Actuators
{
    public static class EndpointServiceCollectionExtensions
    {
        public static IServiceCollection AddBusRefreshActuator(this IServiceCollection services, IConfiguration config)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new ActuatorManagementOptions(config)));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new CloudFoundryManagementOptions(config)));

            var options = new BusRefreshEndpointOptions(config);
            services.TryAddSingleton<IBusRefreshEndpointOptions>(options);
            services.TryAddSingleton<IServerPortAccessor,SimpleServerPortAccessor>();
            services.RegisterEndpointOptions(options);

            services.TryAddSingleton<BusRefreshEndpoint>();
            return services;
        }
    }
}