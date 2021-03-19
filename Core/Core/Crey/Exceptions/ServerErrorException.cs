using System.Net;

namespace Crey.Exceptions
{
    public class ServerErrorException : HttpStatusErrorException
    {
        public ServerErrorException(string message)
            : base(HttpStatusCode.InternalServerError, message)
        {
        }
    }
}
