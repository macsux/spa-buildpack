using System.Threading.Tasks;

namespace SpaLauncher.SinglePageApp
{
    public interface ISpringCloudBusHubClient
    {
        Task NewMessage(string messageType, object message);
    }
}