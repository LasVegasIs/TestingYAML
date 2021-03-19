using System.Net;

namespace Crey.Exceptions
{
    public class AccountNotFoundException : HttpStatusErrorException
    {
        public AccountNotFoundException(string message)
            : base(HttpStatusCode.UnprocessableEntity, message)
        {
        }
    }
}
