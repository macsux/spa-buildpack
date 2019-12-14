using System;
using Newtonsoft.Json;

namespace SpaLauncher.SpringCloudBus.Messages
{
    public  abstract class ApplicationEvent
    {
        [JsonProperty("timestamp", Order = -10)]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    }
}