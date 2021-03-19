using System.Net;

namespace Crey.Exceptions
{
    public class CommunicationErrorException : HttpStatusErrorException
    {
        public CommunicationErrorException(string message)
            : base(HttpStatusCode.InternalServerError, message)
        {
        }
    }
}
