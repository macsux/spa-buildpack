using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpaLauncher.Actuators;
using SpaLauncher.SinglePageApp;
using SpaLauncher.SpringCloudBus;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Env;
using Steeltoe.Management.Endpoint.Trace;
using Steeltoe.Management.Hypermedia;

namespace SpaLauncher
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSpringCloudBus(Configuration);
            services.AddSignalR().AddJsonProtocol();
            services.AddSingleton<AppConfig>();
            services.AddBusRefreshActuator(Configuration);
            services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
            services.AddEnvActuator(Configuration);
//            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddRazorPages();
        }

       
        public void Configure(IApplicationBuilder app)
        {
            app.UseFileServer(new FileServerOptions() { StaticFileOptions = { ServeUnknownFileTypes = true}});
            app.UseTraceActuator(MediaTypeVersion.V2);
            app.UseCloudFoundryActuators(MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
            app.UseEnvActuator();
            app.UseRouting();

            app.UseBusRefreshActuator();
//            app.UseMvc();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapConfigEndpoint();
                endpoints.MapHub<SpringCloudBusHub>("/springCloudBus");
            });
        }
    }
}