using System.Net;

namespace Crey.Instrumentation.Web
{
    public class ItemNotFoundException : HttpStatusErrorException
    {
        public ItemNotFoundException(string message)
            : base(HttpStatusCode.NotFound, message)
        {
        }
    }
}
