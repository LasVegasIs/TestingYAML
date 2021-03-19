using Crey.Instrumentation.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
namespace Crey.Infrastructure
{
    public static class CreySwaggerExtensions
    {

        public static void UseCreyOpenApi(this IApplicationBuilder app, ServiceInfo serviceInfo)
        {
            app
                .UseSwagger(options =>
                {
                    options.PreSerializeFilters.Add((swagger, request) =>
                    {
                        if (app.ApplicationServices.GetRequiredService<IConfiguration>()!.IsRunningInCloud())
                        {
                            // note: with migration to docker/k8s consider hosted dns name to be passed as header or configuration if any issues. for now see frontdoor.tf adds 80 inside, but not outside
                            var referrer = request.Headers[HeaderNames.Referer].FirstOrDefault()?.ToString().Trim();
                            var asHosted = $"{request.Scheme}://{request.Host.Value}";
                            var servers = new List<OpenApiServer>();
                            if (referrer != null)
                            {
                                var referrerUri = new Uri(referrer);
                                var asReffered = $"{referrerUri.Scheme}://{referrerUri.Host}";
                                if (asReffered != asHosted)
                                    servers.Add(new OpenApiServer { Url = asReffered });
                            }
                            servers.Add(new OpenApiServer { Url = asHosted });
                            swagger.Servers = servers;
                        }
                    });
                }
                )
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint($"/swagger/{serviceInfo.Name}/swagger.json", serviceInfo.Name);
                    options.HeadContent += $" changeset = {serviceInfo.Changeset}";
                    // do not set it to root as it conflicts with web sites
                    //options.RoutePrefix = string.Empty;
                    options.ConfigObject.DeepLinking = true;
                    options.ConfigObject.DefaultModelRendering = Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example;
                    options.ConfigObject.DisplayRequestDuration = true;
                    // expands all comments and endpoints by default
                    //options.ConfigObject.DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.Full;
                });
        }

        public static void AddCreyOpenApi(this IServiceCollection services, string service, Assembly? commentsAssembly = null, Action<SwaggerGenOptions>? extraOptions = null)
        {
            services.AddSwaggerGen(options =>
            {
                extraOptions?.Invoke(options);
                options.SwaggerDoc(service, new OpenApiInfo
                {
                    Title = service,
                    TermsOfService = new Uri("https://www.playcrey.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Crey Games",
                        Email = "support@creygames.com",
                        Url = new Uri($"https://playcrey.com/"),
                    }
                });

                options.DescribeAllParametersInCamelCase();
                options.CustomSchemaIds(x => x.FullName);

                var xmlFile = commentsAssembly != null ? $"{commentsAssembly.GetName().Name}.xml" : $"{service}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);

                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            });
        }
    }

}