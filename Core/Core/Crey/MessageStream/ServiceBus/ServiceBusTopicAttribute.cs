using System;

namespace Crey.MessageStream.ServiceBus
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBusTopicAttribute : Attribute
    {
        public string Topic { get; }
        public ServiceBusTopicAttribute(string topic)
        {
            Topic = topic;
        }
    }
}