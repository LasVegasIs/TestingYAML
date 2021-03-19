using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Exceptions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Crey.Web
{
    // moved to standard
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
                            IEnumerable<IPAddress> ipAddresses = context.GetRemoteIPAddresses();
                            telemetry.Context.GlobalProperties.Add("ClientIp", string.Join(",", ipAddresses));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is System.Net.Sockets.SocketException))
                    {
                        logger_.LogError($"Telemetry error: {ex}");
                    }
                }

                await next(context);
            }
            catch (Exception ex)
            {
                if (!(ex is System.Net.Sockets.SocketException))
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
            Error error;
            if (exception is CreyException creyException)
            {
                error = new Error(creyException.Error, msg);
            }
            else
            {
                error = new Error(ErrorCodes.ServerError, msg);
            }

            var result = JsonConvert.SerializeObject(error);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = error.ErrorCode.IntoStatusCode();
            return context.Response.WriteAsync(result);
        }
    }
}
