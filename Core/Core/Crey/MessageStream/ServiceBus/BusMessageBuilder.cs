using Microsoft.Azure.ServiceBus;

namespace Crey.MessageStream.ServiceBus
{
    internal static class MessageExtensions
    {
        public static Message Create(byte[] payload)
        {
            return new Message(payload);
        }

        public static Message AddUserProperty(this Message self, string key, object value)
        {
            self.UserProperties.Add(key, value);
            return self;
        }
    }
}
