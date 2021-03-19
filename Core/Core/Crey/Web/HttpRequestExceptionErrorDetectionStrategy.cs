using System;
using System.Net.Http;
using Microsoft.Rest.TransientFaultHandling;

namespace Crey.Web
{
    // moved to standard
    public class HttpRequestExceptionErrorDetectionStrategy : HttpStatusCodeErrorDetectionStrategy
    {
        public new bool IsTransient(Exception ex)
        {
            return ex is HttpRequestException || base.IsTransient(ex);
        }
    }
}