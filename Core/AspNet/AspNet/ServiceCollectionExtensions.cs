using System;
using Crey.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Crey.AspNet
{
    public static class ApplicationBuilderExtensions
    {
        public static IMvcBuilder AddCreyControllers(this IServiceCollection self)
        {
            return self
                .AddControllers(options =>
                {
                    options.AddBasicChecks();
                    options.Conventions.Add(new AttributeActionHidingConvention<ServerToServerAttribute>());
                })
                .AddNewtonsoftJson()
                .ConfigureApiBehaviorOptions(
                    options =>
                    {
                        options.InvalidModelStateResponseFactory = actionContext =>
                       new BadRequestObjectResult(new { Message = "Model validation error", Detail = actionContext.ModelState });
                    });
        }
    }

}