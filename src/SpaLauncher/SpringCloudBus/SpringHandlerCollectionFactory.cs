using EasyNetQ.Consumer;
using EasyNetQ.Topology;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringHandlerCollectionFactory : IHandlerCollectionFactory
    {
        HandlerCollection _handlerCollection = new HandlerCollection()
        {
            ThrowOnNoMatchingHandler = false
        };
        
        public IHandlerCollection CreateHandlerCollection(IQueue queue) 
            // we're using single queue for all msgs, so we gonna let all handlers take a stab at the incoming msg
        {
            return _handlerCollection;
        }
    }
}