using Crey.Contracts;
using System.Net;

namespace Crey.Exceptions
{
    public class ItemNotFoundException : HttpStatusErrorException
    {
        public ItemNotFoundException(string message)
            : base(HttpStatusCode.NotFound, message)
        {
        }
    }
}
