namespace SpaLauncher.SpringCloudBus.Messages
{
    public class RefreshRemoteApplicationEvent : RemoteApplicationEvent
    {
        public RefreshRemoteApplicationEvent()
        {
        }

        public RefreshRemoteApplicationEvent(string originService, string destinationService) : base(originService, destinationService)
        {
        }
    }
}