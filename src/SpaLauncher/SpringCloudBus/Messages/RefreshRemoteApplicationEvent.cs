namespace SpaLauncher.SpringCloudBus.Messages
{
    public class RefreshRemoteApplicationEvent : RemoteApplicationEvent
    {
        public RefreshRemoteApplicationEvent()
        {
        }

        public RefreshRemoteApplicationEvent(object source, string originService, string destinationService) : base(source, originService, destinationService)
        {
        }
    }
}