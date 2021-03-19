using System;
using System.Net.Http;
using Microsoft.Rest.TransientFaultHandling;

namespace Crey.Instrumentation.Web
{
    public class HttpRequestExceptionErrorDetectionStrategy : HttpStatusCodeErrorDetectionStrategy
    {
        public new bool IsTransient(Exception ex)
        {
            return ex is HttpRequestException || base.IsTransient(ex);
        }
    }
}