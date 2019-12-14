namespace SpaLauncher.SpringCloudBus
{
    public interface IServiceIdResolver
    {
        string ServiceId { get; }
        bool AddressedToMe(string destination);
    }
}