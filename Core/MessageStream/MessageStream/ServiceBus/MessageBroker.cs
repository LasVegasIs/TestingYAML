using Crey.MessageContracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.MessageStream.ServiceBus
{
    public class MessageBroker<TMessageType> : IMessageProducer<TMessageType>
        where TMessageType : IMessageContract
    {
        private class ServiceBusMessage
        {
            // public TMessageType Payload { get; set; }
            public string Payload { get; set; }
        }

        private const string MessageBrokerProperty = "messageBroker";
        private const string BrokerVersionProperty = "brokerVersion";
        private const string MessageTypeProperty = "Type";

        private readonly string _version;
        private readonly ISenderClient _senderClient;
        private readonly MessageSerializer<TMessageType> _serializer;

        public MessageBroker(string channel, IServiceProvider services, ChannelType channelType)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            _serializer = services.GetRequiredService<MessageSerializer<TMessageType>>();

            _version = configuration.GetChangeSetIdentifier();

            var channelName = configuration.GetServiceBusChannelName(channelType, channel);
            var connectionString = configuration.GetServiceBusConnectionString();
            var retryPolicy = new RetryExponential(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), 10);
            switch (channelType)
            {
                case ChannelType.PublishSubscribe: _senderClient = new TopicClient(connectionString, channelName, retryPolicy); break;
                case ChannelType.PointToPoint: _senderClient = new QueueClient(connectionString, channelName, ReceiveMode.PeekLock, retryPolicy); break;
            }
        }

        private Message CreateMessage(TMessageType payload)
        {
            // var data = _serializer.Serialize(new ServiceBusMessage { Payload = payload });
            var data = _serializer.Serialize(new ServiceBusMessage { Payload = _serializer.Serialize(payload) });
            return MessageExtensions.Create(Encoding.UTF8.GetBytes(data))
                .AddUserProperty(MessageBrokerProperty, GetType().Name)
                .AddUserProperty(BrokerVersionProperty, _version)
                .AddUserProperty(MessageTypeProperty, payload.Type);
        }

        public Task SendMessageAsync(TMessageType message)
        {
            return _senderClient.SendAsync(CreateMessage(message));
        }

        public Task SendMessagesAsync(IEnumerable<TMessageType> messages)
        {
            return _senderClient.SendAsync(
                messages
                    .Select(payload => CreateMessage(payload))
                    .ToList()
            );
        }
    }
}
