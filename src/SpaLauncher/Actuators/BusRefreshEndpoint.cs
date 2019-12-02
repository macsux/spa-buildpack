using EasyNetQ;
using SpaLauncher.SpringCloudBus;
using SpaLauncher.SpringCloudBus.Messages;
using Steeltoe.Management.Endpoint;

namespace SpaLauncher.Actuators
{
    public class BusRefreshEndpoint : AbstractEndpoint<string,string>
    {
        private readonly IBus _bus;
        private readonly INodeNameResoler _nameResoler;

        public BusRefreshEndpoint(IBusRefreshEndpointOptions options, IBus bus, INodeNameResoler nameResoler) : base(options)
        {
            _bus = bus;
            _nameResoler = nameResoler;
        }

        public override string Invoke(string destination)
        {
            var msg = new RefreshRemoteApplicationEvent(
                null,
                _nameResoler.ServiceId,
                destination);
            _bus.Publish(msg);
            return string.Empty;
        }
    }
    
}