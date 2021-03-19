using Crey.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;

namespace Crey.Web.Service2Service
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CDNServiceRuleAttribute : Attribute
    {
        public const string ExclusiveForCDN = "ExclusiveForCDN";
        public const string AllowCDN = "AllowCDN";
        public const string DenyCDN = "DenyCDN"; // default if attribute is not present

        private const string CDN_HEADER_NAME = "x-crey-cdn-pass";

        public string Rule { get; set; }

        public CDNServiceRuleAttribute(string rule)
        {
            Rule = rule;
        }

        public static bool IsCDN(HttpContext httpContext)
        {
            var headerKey = httpContext.Request.Headers[CDN_HEADER_NAME];
            return !String.IsNullOrEmpty(headerKey);
        }

        public static void Validate(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();

            var internalAttributes = endpoint?.Metadata.GetOrderedMetadata<CDNServiceRuleAttribute>() ?? Array.Empty<CDNServiceRuleAttribute>();
            Trace.Assert(internalAttributes.Count() <= 1);
            var attrib = internalAttributes.FirstOrDefault();

            bool isCDN = IsCDN(httpContext);

            string rule = attrib?.Rule ?? CDNServiceRuleAttribute.DenyCDN;

            if (rule == ExclusiveForCDN)
            {
                if (!isCDN)
                    throw new AccessDeniedException($"CDN only ep");
            }
            else if (rule == AllowCDN)
            {
                //noop
            }
            else if (isCDN)
            {
                // DenyCDN and everithing else
                throw new AccessDeniedException($"Not a CDN ep");
            }
        }
    }
}
