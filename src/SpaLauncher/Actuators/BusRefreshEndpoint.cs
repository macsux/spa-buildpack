using EasyNetQ;
using SpaLauncher.SpringCloudBus;
using SpaLauncher.SpringCloudBus.Messages;
using Steeltoe.Management.Endpoint;

namespace SpaLauncher.Actuators
{
    public class BusRefreshEndpoint : AbstractEndpoint<string,string>
    {
        private readonly IBus _bus;
        private readonly IServiceIdResolver _serviceIdResolver;

        public BusRefreshEndpoint(IBusRefreshEndpointOptions options, IBus bus, IServiceIdResolver serviceIdResolver) : base(options)
        {
            _bus = bus;
            _serviceIdResolver = serviceIdResolver;
        }

        public override string Invoke(string destination)
        {
            var msg = new RefreshRemoteApplicationEvent(
                _serviceIdResolver.ServiceId,
                destination);
            _bus.Publish(msg, "springCloudBus");
            return string.Empty;
        }
    }
    
}