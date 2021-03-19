using System.Net;

namespace Crey.Exceptions
{
    public class TimeoutException : HttpStatusErrorException
    {
        public TimeoutException(string message)
            : base(HttpStatusCode.RequestTimeout, message)
        {
        }
    }
}
