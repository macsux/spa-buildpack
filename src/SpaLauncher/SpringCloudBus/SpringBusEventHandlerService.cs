using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpaLauncher.SinglePageApp;
using SpaLauncher.SpringCloudBus.Messages;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringBusEventHandlerService : IHostedService
    {
        private readonly IBus _bus;
        private readonly IConfigurationRoot _configuration;
        private readonly ITypeNameSerializer _nameSerializer;
        private readonly IHubContext<SpringCloudBusHub, ISpringCloudBusHubClient> _hub;
        private readonly ILogger<SpringBusEventHandlerService> _log;
        List<ISubscriptionResult> _subscriptions = new List<ISubscriptionResult>();

        public SpringBusEventHandlerService(IBus bus,
            IConfigurationRoot configuration,
            ITypeNameSerializer nameSerializer,
            IHubContext<SpringCloudBusHub, ISpringCloudBusHubClient> hub, 
            ILogger<SpringBusEventHandlerService> log)
        {
            _bus = bus;
            _configuration = configuration;
            _nameSerializer = nameSerializer;
            _hub = hub;
            _log = log;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {

                var subscribed = false;
                var isFailing = false;
                while (!subscribed)
                {
                    try
                    {
                        _subscriptions.Add(_bus.Subscribe<RefreshRemoteApplicationEvent>(nameof(RefreshRemoteApplicationEvent), async e =>
                        {
                            _configuration.Reload();
                            await _hub.Clients.All.NewMessage(_nameSerializer.Serialize(e.GetType()), e);
                        }, cfg => cfg.WithAutoDelete()));
                        _subscriptions.Add(_bus.Subscribe<EnvironmentChangeRemoteApplicationEvent>(nameof(EnvironmentChangeRemoteApplicationEvent), e =>
                        {
                            if (e.Values == null)
                                return;
                            foreach (var env in e.Values)
                            {
                                Environment.SetEnvironmentVariable(env.Key, env.Value);
                            }
                        }, cfg => cfg.WithAutoDelete()));
                        subscribed = true;
                        _log.LogInformation("Successfully connected to Spring Cloud Bus");
                    }
                    catch (Exception)
                    {
                        if(!isFailing)
                            _log.LogError("Unable to connect to RabbitMQ broker. Retrying until works");
                        isFailing = true;
                    }
                }

            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}