using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.FluentConfiguration;
using EasyNetQ.Producer;
using SpaLauncher.SpringCloudBus.Messages;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringRabbitBus : RabbitBus
    {
        private readonly INodeNameResoler _nodeNameResoler;

        public override ISubscriptionResult SubscribeAsync<T>(string subscriptionId,
            Func<T, Task> onMessage, 
            Action<ISubscriptionConfiguration> configure)
        {
            async Task OnSpringMessage(T message)
            {
                if (message is RemoteApplicationEvent springBusMessage && !_nodeNameResoler.AddressedToMe(springBusMessage.DestinationService)) return;
                await onMessage(message);
            }

            return base.SubscribeAsync(subscriptionId, (Func<T, Task>) OnSpringMessage, configure);
        }

        public SpringRabbitBus(IConventions conventions,
            IAdvancedBus advancedBus,
            IExchangeDeclareStrategy exchangeDeclareStrategy,
            IMessageDeliveryModeStrategy messageDeliveryModeStrategy,
            IRpc rpc,
            ISendReceive sendReceive,
            ConnectionConfiguration connectionConfiguration, INodeNameResoler nodeNameResoler) : base(conventions,
            advancedBus,
            exchangeDeclareStrategy,
            messageDeliveryModeStrategy,
            rpc,
            sendReceive,
            connectionConfiguration)
        {
            _nodeNameResoler = nodeNameResoler;
        }
    }
}