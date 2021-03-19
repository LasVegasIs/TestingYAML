using Crey.Contracts;
using System.Net;

namespace Crey.Exceptions
{
    public class AccessDeniedException : HttpStatusErrorException
    {
        public AccessDeniedException(string message)
            : base(HttpStatusCode.Unauthorized, message)
        {
        }
    }
}
