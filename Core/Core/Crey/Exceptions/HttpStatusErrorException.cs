using Crey.Contracts;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;

namespace Crey.Exceptions
{
    [Serializable]
    public class HttpStatusErrorException : Exception
    {
        public readonly HttpStatusCode StatusCode;
        public readonly string BodyJson;

        public HttpStatusErrorException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            Trace.Assert((int)statusCode >= 400); // use it to return errors only (4xx, 5xx)
            StatusCode = statusCode;
            BodyJson = JsonConvert.SerializeObject(new
            {
                Message,
                Detail = (string)null
            });
        }

        public HttpStatusErrorException(HttpStatusCode statusCode, string message, object detail)
           : base(message)
        {
            Trace.Assert((int)statusCode >= 400); // use it to return errors only (4xx, 5xx)
            StatusCode = statusCode;
            BodyJson = JsonConvert.SerializeObject(new
            {
                Message,
                Detail = detail
            });
        }
    }
}
