using System;
using System.Linq;

namespace Crey.MessageContracts
{
#nullable enable
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class MessageTopicAttribute : Attribute
    {
        public string Topic { get; }
        public MessageTopicAttribute(string topic)
        {
            Topic = topic;
        }

        public static MessageTopicAttribute? GetForType<TMessageType>()
        {
            return typeof(TMessageType).GetCustomAttributes(typeof(MessageTopicAttribute), true).SingleOrDefault() as MessageTopicAttribute;
        }

        public static MessageTopicAttribute GetRequiredForType<TMessageType>()
        {
            return GetForType<TMessageType>()
                ?? throw new ContractViolationException($"Missing topic, use {nameof(MessageTopicAttribute)}");
        }
    }
}