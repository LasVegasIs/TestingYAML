using Crey.Configuration.ConfigurationExtensions;
using Crey.Kernel.Authentication;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crey.MessageStream.ServiceBus
{
    public class CreyMessageBroker<TPayload>
        where TPayload : ISentServiceBusMessage
    {
        private class ServiceBusMessage
        {
            public string SenderToken { get; set; }
            public string Payload { get; set; }
        }

        private const string ChangeSetIdentifier = "Changeset";
        private const string MessageBrokerCondition = "messageBroker";
        private const string BrokerVersionCondition = "brokerVersion";
        private const string TypeCondition = "Type";

        private readonly string _version;
        private readonly ISenderClient _senderClient;
        private readonly SessionInfoStore _sessionInfo;

        public CreyMessageBroker(string channel, IServiceProvider services, ChannelType channelType)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var retryPolicy = new RetryExponential(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), 10);

            var connectionString = configuration.GetServiceBusConnectionString();

            var channelName = configuration.GetChannelName(channel);
            _senderClient = channelType switch
            {
                ChannelType.PublishSubscribe => new TopicClient(connectionString, channelName, retryPolicy),
                ChannelType.PointToPoint => new QueueClient(connectionString, channelName, ReceiveMode.PeekLock, retryPolicy),
                _ => throw new Exception($"Unkown channel type {channelType}")
            };

            _sessionInfo = services.GetRequiredService<SessionInfoStore>();
            _version = configuration.GetValue<string>(ChangeSetIdentifier);
        }

        public Task SendMessage(TPayload payload)
        {
            var message = CreateMessage(payload);
            return _senderClient.SendAsync(message);
        }

        public Task SendMessages(IEnumerable<TPayload> payloadList)
        {
            var token = _sessionInfo.GetSignedTokenString();

            var messages = payloadList
                .Select(payload => CreateMessage(payload))
                .ToList();

            return _senderClient.SendAsync(messages);
        }

        private Message CreateMessage(TPayload payload)
        {
            var token = _sessionInfo.GetSignedTokenString();
            // ISSUE: json does not uses settings
            var data = JsonConvert.SerializeObject(new ServiceBusMessage { SenderToken = token, Payload = JsonConvert.SerializeObject(payload) });

            return MessageExtensions.Create(Encoding.UTF8.GetBytes(data))
                .AddUserProperty(MessageBrokerCondition, GetType().Name)
                .AddUserProperty(BrokerVersionCondition, _version)
                .AddUserProperty(TypeCondition, payload.Type);
        }
    }
}
