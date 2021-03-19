using System;

namespace Crey.MessageStream.ServiceBus
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBusQueueAttribute : Attribute
    {
        public string Queue { get; }
        public ServiceBusQueueAttribute(string queue)
        {
            Queue = queue;
        }
    }
}