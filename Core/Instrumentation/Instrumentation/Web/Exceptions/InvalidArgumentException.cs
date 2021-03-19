using System.Net;

namespace Crey.Instrumentation.Web
{
    public class InvalidArgumentException : HttpStatusErrorException
    {
        public InvalidArgumentException(string message)
            : base(HttpStatusCode.PreconditionFailed, message)
        {
        }
    }
}
