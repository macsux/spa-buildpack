using System;

namespace SpaLauncher.SpringCloudBus.Messages
{
    public  abstract class ApplicationEvent : EventObject
    {
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public ApplicationEvent(object source) : base(source)
        {
        }
    }
}