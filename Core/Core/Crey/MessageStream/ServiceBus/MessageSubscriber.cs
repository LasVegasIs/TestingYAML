using Crey.Configuration.ConfigurationExtensions;
using Crey.Exceptions;
using Crey.Kernel.Authentication;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prometheus;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.MessageStream.ServiceBus
{
    /// <summary>
    /// Defines:
    /// 1. Messages which are received/filtered (topic/queue/filter)
    /// 2. Defines deserialization of bodies.
    /// 3. Adds retryability.
    /// </summary>
    public class MessageSubscriber<TMessageFactory, TPayload>
        where TMessageFactory : class, IMessageFactory<TPayload>
        where TPayload : class, IReceivedServiceBusMessage
    {
        private static Histogram servicebus_request_duration_seconds =
            Metrics.CreateHistogram(
                    nameof(servicebus_request_duration_seconds),
                    "Service bus message handling",
                    nameof(IStreamedMessage.Type)
                    );

        private class ServiceBusMessage
        {
            public string SenderToken { get; set; }
            public JObject Payload { get; set; }
        }

        private const string DateTimeFormat = "yyyy.MM.dd";

        private readonly string _service;
        private readonly IReceiverClient _receiverClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessageReceiver _deadLetterQueue;
        private readonly ISenderClient _senderClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private byte _retryDeadCount;

        public MessageSubscriber(string subscriptionName, string channel, IServiceProvider serviceProvider, ChannelType channelType)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var retryPolicy = new RetryExponential(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), 10);

            var connectionString = _configuration.GetServiceBusConnectionString();

            var channelName = _configuration.GetChannelName(channel);
            switch (channelType)
            {
                case ChannelType.PublishSubscribe:
                    _receiverClient = new SubscriptionClient(connectionString, channelName, subscriptionName, retryPolicy: retryPolicy);
                    _senderClient = new TopicClient(connectionString, channelName, retryPolicy);
                    break;
                case ChannelType.PointToPoint:
                    var queueClient = new QueueClient(connectionString, channelName, ReceiveMode.PeekLock, retryPolicy);
                    _receiverClient = queueClient;
                    _senderClient = queueClient;
                    break;
                default:
                    throw new Exception($"Unkown channel type {channelType}");
            }

            _logger = serviceProvider.GetService<ILogger<MessageSubscriber<TMessageFactory, TPayload>>>();
            _serviceProvider = serviceProvider;

            string path = $"{channelName}/Subscriptions/{subscriptionName}";
            string deadPath = $"{path}/$DeadLetterQueue";
            _deadLetterQueue = new MessageReceiver(connectionString, deadPath, ReceiveMode.PeekLock);
            _retryDeadCount = _configuration.GetValue<byte>("ServiceBusDeadLetterRetryCount", 10);

            _service = $"{subscriptionName}-{channel}";
        }

        public async Task RegisterSubscriber()
        {
            if (!_configuration.GetValue<bool>("CodeFirstServiceBus", false))
            {
                await AddFilters();
            }

            _receiverClient.RegisterMessageHandler(ProcessMessagesAsync, new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            });
        }

        // to view
        public async Task<IEnumerable<Message>> GetRawMessagesFromDeadLetterQueue(byte count)
        {
            var messages = await _deadLetterQueue.PeekAsync(count);
            return messages;
        }

        public async Task<int> DeadLetterQueue(byte count, TimeSpan timeout)
        {
            var messages = await _deadLetterQueue.ReceiveAsync(count, timeout);
            if (messages == null) return 0;
            foreach (var message in messages)
            {
                var retryCount = message.UserProperties.TryGetValue("CreyRetryCount", out var value) ? (byte)(int)value : 1;
                if (retryCount > _retryDeadCount)
                {
                    _logger.LogCritical("Retry count to handle message {MessageId} from dead letter queue reached limit", message.MessageId);
                    throw new ServerErrorException("Retry failed");
                }
                var cloned = message.Clone();
                cloned.UserProperties["CreyRetryCount"] = retryCount;
                await _senderClient.SendAsync(cloned);
                await _deadLetterQueue.CompleteAsync(message.SystemProperties.LockToken);
            }
            return messages.Count;
        }

        public async Task<int> ProcessDeadLetterQueue(byte count, TimeSpan timeout)
        {
            var messages = await _deadLetterQueue.ReceiveAsync(count, timeout);
            if (messages == null) return 0;
            foreach (var message in messages)
            {
                try
                {
                    await HandleMessageAsync(message, new CancellationToken());
                    await _deadLetterQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Processing dead letter failed");
                }
            }
            return messages.Count;
        }

        public Task CompleteMessage(string lockToken)
        {
            return _deadLetterQueue.CompleteAsync(lockToken);
        }

        public async Task HandleMessageAsync(Message message, CancellationToken token)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;
                var payload = ReadMessage(message, scopedProvider);

                var context = new ValidationContext(payload, serviceProvider: scopedProvider, items: null);
                Validator.ValidateObject(payload, context, true);

                using (servicebus_request_duration_seconds.WithLabels(payload.Type).NewTimer())
                {
                    await payload.Handle(scopedProvider);
                }
            }
        }

        private TPayload ReadMessage(Message message, IServiceProvider serviceProvider)
        {
            var messageFactory = serviceProvider.GetRequiredService<TMessageFactory>();
            var sessionInfoStore = serviceProvider.GetRequiredService<SessionInfoStore>();

            ServiceBusMessage receivedMessage;
            using (var stringReader = new StringReader(Encoding.UTF8.GetString(message.Body)))
            using (var reader = new JsonTextReader(stringReader))
            {
                var serializer = new JsonSerializer();
                var bodyObject = serializer.Deserialize<JObject>(reader);
                var rawPayload = bodyObject.GetValue("Payload");
                if (rawPayload.Type == JTokenType.String)
                {
                    using var payloadStringReader = new StringReader(rawPayload.ToString());
                    using var payloadReader = new JsonTextReader(payloadStringReader);
                    receivedMessage = new ServiceBusMessage
                    {
                        SenderToken = bodyObject.GetValue("SenderToken").ToString(),
                        Payload = serializer.Deserialize<JObject>(payloadReader)
                    };
                }
                else if (rawPayload.Type == JTokenType.Object)
                {
                    receivedMessage = new ServiceBusMessage
                    {
                        Payload = (JObject)rawPayload
                    };
                }
                else
                {
                    throw new Exceptions.ServerErrorException($"Unexpected payload: {rawPayload.ToString()}");
                }
            }

            // if we have a token trust it
            if (!string.IsNullOrEmpty(receivedMessage.SenderToken))
            {
                sessionInfoStore.SetFromSignedToken(receivedMessage.SenderToken);
            }
            var payload = messageFactory.Deserialize(receivedMessage.Payload)
                ?? throw new Exceptions.ServerErrorException($"Unknown SB payload: {receivedMessage.Payload}");

            return payload;
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            await HandleMessageAsync(message, token);
            await _receiverClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        public async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            var exception = exceptionReceivedEventArgs.Exception;
            _logger.LogWarning(exception, $"Message error received: {exception.Message}\n Context: {context}");

            await Task.CompletedTask;
        }

        private async Task AddFilters()
        {
            var subscriptionClient = _receiverClient as SubscriptionClient;
            if (subscriptionClient == null)
            {
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;
                var messageFactory = scopedProvider.GetRequiredService<TMessageFactory>();

                var currentRules = await subscriptionClient.GetRulesAsync();

                if (currentRules.All(rule => ParseRuleName(rule) < messageFactory.CurrentVersion))
                {
                    // First we add a rule so new messages can qualify
                    var typeFilters = string.Join(",", messageFactory.TypeMap.Keys.Select(type => $"'{type}'"));
                    var compiledFilterString = $"(To = '{_service}' OR To IS NULL) AND (Type IN ({typeFilters}))";
                    await subscriptionClient.AddRuleAsync(
                        messageFactory.CurrentVersion.ToString(DateTimeFormat),
                        new SqlFilter(compiledFilterString));

                    // Remove each rule that are older than the one we just added
                    currentRules = await subscriptionClient.GetRulesAsync();
                    foreach (var rule in currentRules)
                    {
                        if (ParseRuleName(rule) < messageFactory.CurrentVersion)
                        {
                            await subscriptionClient.RemoveRuleAsync(rule.Name);
                        }
                    }
                }
            }
        }

        private static DateTime ParseRuleName(RuleDescription rule)
        {
            return DateTime.TryParseExact(rule.Name, DateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var result) ? result : DateTime.MinValue;
        }
    }
}