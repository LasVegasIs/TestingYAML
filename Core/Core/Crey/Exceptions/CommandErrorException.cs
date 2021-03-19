using System.Net;

namespace Crey.Exceptions
{
    public class CommandErrorException<T> : HttpStatusErrorException
    {
        public CommandErrorException(string message)
            : base(HttpStatusCode.BadRequest, message)
        {
        }

        public CommandErrorException(string message, T errorDetail)
            : base(HttpStatusCode.BadRequest, message, errorDetail)
        {
        }
    }
}
