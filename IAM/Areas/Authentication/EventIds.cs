using Microsoft.Extensions.Logging;

namespace IAM.Areas.Authentication
{
    public class EventIds
    {
        public static readonly EventId CreyTicketApplied = new EventId(777, nameof(CreyTicketApplied));
    }
}