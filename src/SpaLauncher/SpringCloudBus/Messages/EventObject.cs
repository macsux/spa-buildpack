namespace SpaLauncher.SpringCloudBus.Messages
{
    public abstract class EventObject
    {
        public EventObject(object source)
        {
            Source = source;
        }

        public object Source { get; set; }
    }
}