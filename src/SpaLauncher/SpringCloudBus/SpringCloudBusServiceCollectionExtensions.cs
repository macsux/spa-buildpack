using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaLauncher.SpringCloudBus.Util;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
using IConnectionFactory = RabbitMQ.Client.IConnectionFactory;

namespace SpaLauncher.SpringCloudBus
{
    public static class SpringCloudBusServiceCollectionExtensions
    {
        public static IServiceCollection AddSpringCloudBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ => (IConfigurationRoot) configuration);
            
            services.AddRabbitMQConnection(configuration);
            services.RegisterEasyNetQ(svc =>
            {
                using var scope = svc.CreateScope();
                var rabbitConnectionFactory = scope.Resolve<IConnectionFactory>();
                return new ConnectionConfiguration()
                {
                    AMQPConnectionString = rabbitConnectionFactory.Uri,
                };
            });    
            
            // override default behavior of EasyNetQ to conform to Spring Cloud Bus
//            services.AddSingleton<ISerializer, SpringJsonSerializer>();
            services.AddSingleton<ISerializer>(new JsonSerializer(SpringJsonSerializerSettings.Instance));
            services.AddSingleton<ITypeNameSerializer, SpringTypeNameSerializer>();
            services.AddSingleton<IConventions, SpringConventions>();
            services.AddSingleton<INodeNameResoler, SpringNodeNameResoler>();
            services.AddSingleton<IBus, SpringRabbitBus>();
            services.AddSingleton<ServerPortAccessor>();
            services.AddSingleton<IMessageSerializationStrategy, SpringMessageSerializationStrategy>();
            
            services.AddSingleton<IHostedService, SpringBusEventHandlerService>(); // this is where all the message handlers live
            return services;
        }
    }
}