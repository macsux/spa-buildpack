namespace SpaLauncher.SpringCloudBus
{
    public interface INodeNameResoler
    {
        string ServiceId { get; }
        bool AddressedToMe(string destination);
    }
}