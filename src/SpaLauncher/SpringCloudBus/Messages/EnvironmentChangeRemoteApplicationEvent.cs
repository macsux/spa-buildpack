using System.Collections.Generic;

namespace SpaLauncher.SpringCloudBus.Messages
{
    public class EnvironmentChangeRemoteApplicationEvent : RemoteApplicationEvent
    {
        public Dictionary<string,string> Values { get; set; }
    }
}