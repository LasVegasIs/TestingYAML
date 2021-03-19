using Crey.Instrumentation.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Crey.Instrumentation.Web
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlingMiddleware> logger_;
        private readonly IConfiguration configuration_;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IConfiguration configuration)
        {
            this.next = next;
            logger_ = logger;
            configuration_ = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // log the client IP
                try
                {
                    var telemetry = context.Features.Get<RequestTelemetry>();
                    if (telemetry != null)
                    {
                        if (context.Connection.RemoteIpAddress != null)
                        {
                            IEnumerable<IPAddress> ipAddresses = GetRemoteIPAddresses(context);
                            telemetry.Context.GlobalProperties.Add("ClientIp", string.Join(",", ipAddresses));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is SocketException))
                    {
                        logger_.LogError($"Telemetry error: {ex}");
                    }
                }

                await next(context);
            }
            catch (Exception ex)
            {
                if (!(ex is SocketException))
                {
                    logger_.LogError($"Server error: {ex}");
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception is HttpStatusErrorException backendException)
            {
                var code = (int)backendException.StatusCode;
                if (code >= 500 && code < 600)
                {
                    var properties = new Dictionary<string, string> { };
                    var measurements = new Dictionary<string, double> { };
                    var telemetry = new TelemetryClient();
                    telemetry.TrackException(exception, properties, measurements);
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = code;
                return context.Response.WriteAsync(backendException.BodyJson);
            }

            string msg = configuration_.IsTestingSlot() ? $"{exception}" : exception.Message;

            var result = JsonConvert.SerializeObject(new InternalServerErrorException(msg));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(result);
        }

        /// <exception cref="InvalidArgumentException">Fail</exception>
        private static IEnumerable<IPAddress> GetRemoteIPAddresses(HttpContext httpContext)
        {
            var result = new List<IPAddress>();
            if (httpContext.Connection.RemoteIpAddress != null)
                result.Add(httpContext.Connection.RemoteIpAddress);

            string? failed = null;
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var ipList) && ipList.Any())
            {
                var ips = ipList.Select(x => x.Split(",")).SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x));
                foreach (var ip in ips)
                {
                    // ISSUE: .NET Core 5 has proper attributes for this pattern `when true than not null` then no warning, but with standard need to `!`
                    if (CreyIPAddress.TryParse(ip, out var parsed))
                        result.Add(parsed!);
                    else if (failed == null)
                        failed = string.Join(",", ips);
                }
            }

            if (!result.Any() && failed != null)
                throw new InvalidArgumentException($"Failed to parse IP from one of:'{failed}'");
            return result;
        }
    }
}
