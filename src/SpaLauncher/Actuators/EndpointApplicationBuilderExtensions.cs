using Microsoft.AspNetCore.Builder;

namespace SpaLauncher.Actuators
{
    public static class EndpointApplicationBuilderExtensions
    {
        public static  IApplicationBuilder UseBusRefreshActuator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BusRefreshEndpointMiddleware>();
        }
    }
}