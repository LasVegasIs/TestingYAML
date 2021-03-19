using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Crey.Utils;

namespace Crey.Instrumentation.Web
{
    public static class BinaryContentExtensions
    {
        public static ActionResult IntoActionResult(this BinaryContent content)
        {
            if (content == null)
                return new StatusCodeResult(StatusCodes.Status422UnprocessableEntity);

            if (content.Data == null)
                return new StatusCodeResult(StatusCodes.Status304NotModified);

            var mime = content.MimeType ?? "application/octet-stream";
            var res = new FileContentResult(content.Data, mime);
            if (!string.IsNullOrEmpty(content.ContentHash))
                res.EntityTag = new EntityTagHeaderValue($"\"{content.ContentHash}\"");

            return res;
        }
    }
}