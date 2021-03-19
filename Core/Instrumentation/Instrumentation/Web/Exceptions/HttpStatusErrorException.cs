using Newtonsoft.Json;
using System;
using System.Net;

namespace Crey.Instrumentation.Web
{
    [Serializable]
    public class HttpStatusErrorException : Exception
    {
        public readonly HttpStatusCode StatusCode;

        public object? Detail { get; set; }
        public string BodyJson => JsonConvert.SerializeObject(new { Message, Detail });

        public HttpStatusErrorException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            if ((int)statusCode < 400)
            {
                throw new InvalidProgramException($"{nameof(HttpStatusErrorException)} must be used only for errors");
            }
            StatusCode = statusCode;
        }

        public HttpStatusErrorException WithDetail(object detail)
        {
            Detail = detail;
            return this;
        }
    }
}
