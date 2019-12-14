using System;

namespace SpaLauncher.SpringCloudBus.Messages
{
    public abstract class RemoteApplicationEvent : ApplicationEvent
    {
        public string OriginService { get; set; }
        public string DestinationService { get; set; }
        public Guid Id { get; set; }

        protected RemoteApplicationEvent() : this(null, null)
        {
        }
        protected RemoteApplicationEvent(string originService, string destinationService)
        {
            OriginService = originService;
            if (string.IsNullOrEmpty(destinationService)) {
                destinationService = "**";
            }

            
            if (!"**".Equals(destinationService) && destinationService.Split(":").Length <= 2 && !destinationService.EndsWith(":**", StringComparison.InvariantCultureIgnoreCase))
            {
                destinationService = destinationService + ":**";
            }

            DestinationService = destinationService;
            Id = Guid.NewGuid();
        }
    }

}