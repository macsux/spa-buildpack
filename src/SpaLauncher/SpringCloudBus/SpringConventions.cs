using EasyNetQ;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringConventions : Conventions
    {
        public SpringConventions(ITypeNameSerializer typeNameSerializer) : base(typeNameSerializer)
        {
            ExchangeNamingConvention = type => "springCloudBus";
//            TopicNamingConvention = type => "";
            QueueNamingConvention = (msgType, subscriptionId) => subscriptionId;
        }
    }
}