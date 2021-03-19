using System.Net;

namespace Crey.Exceptions
{
    // note: moved to standard
    public class InvalidArgumentException : HttpStatusErrorException
    {
        public InvalidArgumentException(string message)
            : base(HttpStatusCode.PreconditionFailed, message)
        {
        }
    }
}
