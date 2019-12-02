using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SpaLauncher.SinglePageApp
{
    public static class ConfigEndpointExtensions
    {
        public static IEndpointConventionBuilder MapConfigEndpoint(this IEndpointRouteBuilder endpoints)
        {
            var config = endpoints.ServiceProvider.GetRequiredService<AppConfig>();
            return endpoints.MapGet(
                pattern: "/config",
                requestDelegate: async context => await context.Response.WriteAsync(config.GetConfigJson()));
        }
    }
}