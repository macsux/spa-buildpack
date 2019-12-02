using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace SpaLauncher.SpringCloudBus.Util
{
    public class ServerPortAccessor
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