using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prometheus;
using System;
using System.Diagnostics;
using System.Text;
//using System.Text.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Crey.Infrastructure
{

    public static class EndpointRouteBuilderExtensions
    {
        public static void MapCreyInfrastructure(this IEndpointRouteBuilder self, ServiceInfo info)
        {
            self.MapMetrics();
            self.MapInfoDetailsLive(info);
        }

        public static void MapInfoDetailsLive(this IEndpointRouteBuilder self, ServiceInfo info)
        {
            DateTimeOffset startTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
            self.MapMethods("/info/detail", new[] { HttpMethods.Head, HttpMethods.Get }, async context => await WriteInfo(info, context, startTime));
            self.MapMethods("/api/v1/info/detail", new[] { HttpMethods.Head, HttpMethods.Get }, async context => await WriteInfo(info, context, startTime));
        }

        private static async Task WriteInfo(ServiceInfo info, HttpContext context, DateTimeOffset startTime)
        {
            context.Response.Headers.Add(HeaderNames.CacheControl, "no-store,no-cache");
            context.Response.Headers.Add(HeaderNames.Pragma, "no-cache");
            context.Response.Headers.Add("crey-service-start-time", startTime.ToString());

            context.Response.ContentType = MimeMapping.KnownMimeTypes.Json;
            //var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var json = JsonConvert.SerializeObject(info, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() } });
            var body = Encoding.UTF8.GetBytes(json);
            context.Response.ContentLength = body.Length;
            await context.Response.BodyWriter.WriteAsync(body);
        }
    }
}