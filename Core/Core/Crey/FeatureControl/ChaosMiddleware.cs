using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Kernel.ServiceDiscovery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    public class ChaosMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string deploymentSlot_;

        public ChaosMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            deploymentSlot_ = configuration.GetDeploymentSlot();
        }

        public async Task Invoke(HttpContext context, ILogger<ChaosMiddleware> logger, ICreyService<IFeatureGate> featureGate)
        {
            if (await featureGate.Value.IsChaosAsync(context.Request.Path))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Chaos rules all");
                return;
            }

            await next(context);
        }
    }


}
