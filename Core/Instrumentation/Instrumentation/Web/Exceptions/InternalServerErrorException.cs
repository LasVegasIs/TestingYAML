using System.Net;

namespace Crey.Instrumentation.Web
{
    public class InternalServerErrorException : HttpStatusErrorException
    {
        public InternalServerErrorException(string message)
            : base(HttpStatusCode.InternalServerError, message)
        {
        }
    }
}
