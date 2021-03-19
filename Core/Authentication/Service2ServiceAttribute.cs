using Crey.Instrumentation.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Crey.Authentication
{
    public class Service2ServicePolicy
    {
        public string HeaderName { get; set; }
        public string KeyName { get; set; }  // key of the secret in the vault

        public const string Internal = "InternalPolicy";
        public static Service2ServicePolicy InternalPolicy = new Service2ServicePolicy
        {
            HeaderName = "crey-internal-key",
            KeyName = "IntraServiceKey",
        };

        public const string Metric = "MetricPolicy";
        public static Service2ServicePolicy MetricPolicy = new Service2ServicePolicy
        {
            HeaderName = "metric-internal-key",
            KeyName = "MetricServiceKey",
        };

        public const string GameServer = "GameServerPolicy";
        public static Service2ServicePolicy GameServerPolicy = new Service2ServicePolicy
        {
            HeaderName = "gameserver-internal-key",
            KeyName = "GameServerInternalKey",
        };

        public const string Multiplay = "MultiplayImagesPolicy";
        public static Service2ServicePolicy MultiplayImagesPolicy = new Service2ServicePolicy
        {
            HeaderName = "multiplayimages-internal-key",
            KeyName = "MultiplayImagesInternalKey",
        };
    }

    public static class Service2ServicePolicyExtensions
    {
        public static void AddService2ServiceHeaderPolicy(this HttpRequestMessage httpRequest, IConfiguration config, Service2ServicePolicy policy)
        {
            var apiKey = config.GetValue<string>(policy.KeyName);
            httpRequest.Headers.Add(policy.HeaderName, apiKey);
        }

        public static void AddService2ServiceHeaderPolicy(this List<KeyValuePair<string, string>> headers, IConfiguration config, Service2ServicePolicy policy)
        {
            var apiKey = config.GetValue<string>(policy.KeyName);
            headers.Add(new KeyValuePair<string, string>(policy.HeaderName, apiKey));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ServerToServerAttribute : Attribute
    {
        public string HeaderName { get; set; }
        public string KeyName { get; set; }

        public ServerToServerAttribute()
        {
            Setup(Service2ServicePolicy.InternalPolicy);
        }

        public ServerToServerAttribute(string policy)
        {
            switch (policy)
            {
                case Service2ServicePolicy.Internal:
                    Setup(Service2ServicePolicy.InternalPolicy);
                    break;
                case Service2ServicePolicy.Metric:
                    Setup(Service2ServicePolicy.MetricPolicy);
                    break;
                case Service2ServicePolicy.GameServer:
                    Setup(Service2ServicePolicy.GameServerPolicy);
                    break;
                case Service2ServicePolicy.Multiplay:
                    Setup(Service2ServicePolicy.MultiplayImagesPolicy);
                    break;

                default:
                    throw new HttpStatusErrorException(HttpStatusCode.InternalServerError, $"Invalid policy: {policy}");
            }
        }

        private void Setup(Service2ServicePolicy policy)
        {
            HeaderName = policy.HeaderName;
            KeyName = policy.KeyName;
            Trace.Assert(!string.IsNullOrEmpty(HeaderName));
            Trace.Assert(!string.IsNullOrEmpty(KeyName));
        }

        public static void Validate(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();

            var internalAttributes = endpoint?.Metadata.GetOrderedMetadata<ServerToServerAttribute>() ?? Array.Empty<ServerToServerAttribute>();
            Trace.Assert(internalAttributes.Count() <= 1);
            var attrib = internalAttributes.FirstOrDefault();
            if (attrib != null)
            {
                var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                var headerKey = httpContext.Request.Headers[attrib.HeaderName];
                var requiredKey = config.GetValue<string>(attrib.KeyName);
                if (HttpUtility.UrlEncode(requiredKey) != requiredKey)
                    throw new HttpStatusErrorException(HttpStatusCode.InternalServerError, $"Key containes invalid characters");

                if (headerKey != requiredKey)
                    throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, $"Invalid api key {attrib.HeaderName}");
            }
        }
    }
}
