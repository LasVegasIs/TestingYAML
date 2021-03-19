
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Configuration;
using Crey.Configuration.ConfigurationExtensions;

namespace Microsoft.AspNetCore.Builder
{

    // note: moved to net50
    public static class CreySwaggerExtensions
    {
        public static void UseCreySwagger(this IApplicationBuilder app, string service, string changeset = null)
        {
            app
                .UseSwagger(options =>
                {
                    options.PreSerializeFilters.Add((swagger, request) =>
                    {
                        if (app.ApplicationServices.GetService<IConfiguration>().IsRunningInCloud())
                        {
                            // note: with migration to docker/k8s consider hosted dns name to be passed as header or configuration if any issues. for now see frontdoor.tf adds 80 inside, but not outside
                            var referrer = request.Headers["Referer"].FirstOrDefault()?.ToString().Trim();
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
                    options.SwaggerEndpoint($"/swagger/{service}/swagger.json", service);

                    options.HeadContent += $" changeset = {changeset}";
                    // do not set it to root as it conflicts with web sites
                    //options.RoutePrefix = string.Empty;
                    options.ConfigObject.DeepLinking = true;
                    options.ConfigObject.DefaultModelRendering = Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example;
                    options.ConfigObject.DisplayRequestDuration = true;
                    // expands all coments and endpoints by default
                    //options.ConfigObject.DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.Full;
                });
        }

        public static void AddCreySwagger(this IServiceCollection services, string service, string slot)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(service, new OpenApiInfo
                {
                    Title = service,
                    Description = service + " API",
                    TermsOfService = new Uri("https://www.playcrey.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Crey Games",
                        Email = "support@creygames.com",
                        Url = new Uri($"https://{slot}.playcrey.com/"),
                    }
                });
                options.DescribeAllParametersInCamelCase();
                options.CustomSchemaIds(x => x.FullName);
                var xmlFile = $"{service}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });
        }
    }

}