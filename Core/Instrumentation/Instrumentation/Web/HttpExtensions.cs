using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Crey.Instrumentation.Web
{
    public static class HttpExtensions
    {
        public static string? GetETag(this HttpRequest httpRequest)
        {
            StringValues eTag;
            if (httpRequest.Headers.TryGetValue(HeaderNames.IfNoneMatch, out eTag))
            {
                return eTag.ToString();
            }

            return null;
        }
    }
}
