using System.Threading.Tasks;

namespace SpaLauncher.SinglePageApp
{
    public interface ISpringCloudBusHubServer
    {
        Task SendMessage(object message);
    }
}