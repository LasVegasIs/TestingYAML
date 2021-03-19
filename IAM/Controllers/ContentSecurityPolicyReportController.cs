using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IAM.Controllers
{
    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy/report-uri
    [EnableCors]
    public class ContentSecurityPoilicyReportController : Controller
    {
        public class ContentSecurityPolicyReportRequest
        {
            [JsonProperty(PropertyName = "csp-report")]
            public ContentSecurityPolicyReport CspReport { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class ContentSecurityPolicyReport
        {
            [JsonProperty(PropertyName = "document-uri")]
            public string DocumentUri { get; set; }

            [JsonProperty(PropertyName = "referrer")]
            public string Referrer { get; set; }

            [JsonProperty(PropertyName = "violated-directive")]
            public string ViolatedDirective { get; set; }

            [JsonProperty(PropertyName = "effective-directive")]
            public string EffectiveDirective { get; set; }

            [JsonProperty(PropertyName = "original-policy")]
            public string OriginalPolicy { get; set; }

            [JsonProperty(PropertyName = "blocked-uri")]
            public string BlockedUri { get; set; }

            [JsonProperty(PropertyName = "status-code")]
            public int StatusCode { get; set; }
        }

        private readonly ILogger logger_;

        public ContentSecurityPoilicyReportController(ILogger<ContentSecurityPoilicyReportController> logger)
        {
            logger_ = logger;
        }

        [HttpPost("/iam/cspreport")]
        public IActionResult CspReport([FromBody]ContentSecurityPolicyReportRequest request)
        {
            logger_.LogCritical($"Content Security Policy Violation: {request.ToString()}");
            return Ok();
        }
    }
}
