using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Middleware;

namespace SpaLauncher.Actuators
{
    public class BusRefreshEndpointMiddleware : EndpointMiddleware<string,string>
    {
        private readonly RequestDelegate _next;

        public BusRefreshEndpointMiddleware(RequestDelegate next,
            BusRefreshEndpoint endpoint,
            IEnumerable<IManagementOptions> mgmtOptions,
            ILogger<BusRefreshEndpointMiddleware> logger = null)
            : base(endpoint, mgmtOptions, logger: logger, allowedMethods: new []{ HttpMethod.Post })
        {
            _next = next;
            _exactRequestPathMatching = false;
        }
        
        public async Task Invoke(HttpContext context, BusRefreshEndpoint endpoint)
        {
            
            _endpoint = endpoint;

            if (RequestVerbAndPathMatch(context.Request.Method, context.Request.Path.Value))
            {
                await HandleBusRefreshRequestAsync(context).ConfigureAwait(false);
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }
        protected internal async Task HandleBusRefreshRequestAsync(HttpContext context)
        {
            var serialInfo = DoRequest(context);
            _logger?.LogDebug("Returning: {0}", serialInfo);
            context.Response.Headers.Add("Content-Type", "application/vnd.spring-boot.actuator.v2+json");
            await context.Response.WriteAsync(serialInfo).ConfigureAwait(false);
        }

        protected internal string DoRequest(HttpContext context)
        {
            
            var basePath = _mgmtOptions.Where(x => context.Request.Path.StartsWithSegments(new PathString(x.Path))).First().Path;
            if (!basePath.EndsWith("/") && !string.IsNullOrEmpty(_endpoint.Path))
                basePath += "/";
            basePath += _endpoint.Path;
            var destination = context.Request.Path.Value.Remove(0, basePath.Length).Trim('/');
            return _endpoint.Invoke(destination);
        }
    }
}