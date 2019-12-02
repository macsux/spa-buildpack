using System;

namespace SpaLauncher.SpringCloudBus.Messages
{
    public abstract class RemoteApplicationEvent : ApplicationEvent
    {
        private static object TRANSIENT_SOURCE = new object();

        public string OriginService { get; set; }
        public string DestinationService { get; set; }
        public Guid Id { get; set; }

        protected RemoteApplicationEvent() : this(TRANSIENT_SOURCE,  null, null)
        {
        }
        protected RemoteApplicationEvent(object source, string originService, string destinationService) : base(source)
        {
            OriginService = originService;
            if (destinationService == null) {
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