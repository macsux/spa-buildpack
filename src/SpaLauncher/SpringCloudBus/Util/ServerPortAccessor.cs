using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace SpaLauncher.SpringCloudBus.Util
{
    public interface IServerPortAccessor
    {
        int Port { get; }
    }

    public class SimpleServerPortAccessor : IServerPortAccessor
    {
        public int Port
        {
            get
            {
//                var portStr = Environment.GetEnvironmentVariable("PORT");
//                if (portStr != null && int.TryParse(portStr, out var port))
//                    return port;
//                return 8080;
                return 0;
            }
        }
    }


    public class ServerPortAccessor : IServerPortAccessor
    {
        private readonly IServer _server;
        private readonly Lazy<int> _port;

        public ServerPortAccessor(IServer server)
        {
            _server = server;
            _port = new Lazy<int>(() =>
            {
                var address = _server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
                var match = Regex.Match(address, @"^.+:(\d+)$");
                if (match.Success)
                {
                    return Int32.Parse(match.Groups[1].Value);
                }

                return 0;
            });
        }


        public int Port => _port.Value;
    }
}