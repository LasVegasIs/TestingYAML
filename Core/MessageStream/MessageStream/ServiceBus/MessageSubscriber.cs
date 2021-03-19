using Crey.MessageContracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Prometheus;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.MessageStream.ServiceBus
{
    /// <summary>
    /// Defines:
    /// 1. Messages which are received/filtered (topic/queue/filter)
    /// 2. Defines deserialization of bodies.
    /// 3. Adds retryability.
    /// </summary>
    public class MessageSubscriber<TMessageType> : IMessageConsumer<TMessageType>
        where TMessageType : class, IMessageContract
    {
        private static Histogram servicebus_consume_message_duration_seconds =
            Metrics.CreateHistogram(
                    nameof(servicebus_consume_message_duration_seconds),
                    "Service bus consume message handling",
                    "Type"
                    );

        private const string DateTimeFormat = "yyyy.MM.dd";

        private readonly string _subscriptionName;
        private readonly string _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly MessageSerializer<TMessageType> _serializer;

        private IReceiverClient _receiverClient;
        private ISenderClient _senderClient;
        private MessageReceiver _deadLetterQueue;

        public MessageSubscriber(string subscriptionName, string channel, ChannelType channelType, IServiceProvider serviceProvider)
        {
            _subscriptionName = subscriptionName;
            _channel = channel;
            _serviceProvider = serviceProvider;
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _serializer = serviceProvider.GetRequiredService<MessageSerializer<TMessageType>>();
            _logger = serviceProvider.GetService<ILogger<MessageSubscriber<TMessageType>>>();

            if (!_configuration.IsCodeFirstServiceBus())
            {
                InitializeClients(channelType);
            }
        }

        public async Task RegisterSubscriber(ChannelType channelType)
        {
            if (_configuration.IsCodeFirstServiceBus())
            {
                InitializeClients(channelType);
            }
            else
            {
                await AddFilters();
            }

            _receiverClient.RegisterMessageHandler(
                ProcessMessagesAsync,
                new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false
                });
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

        public async Task<int> ResendDeadLettersIntoQueue(byte count, TimeSpan timeout, int retryCount)
        {
            var messages = await _deadLetterQueue.ReceiveAsync(count, timeout);
            if (messages == null) return 0;
            foreach (var message in messages)
            {
                var currentRetryCount = message.UserProperties.TryGetValue("CreyRetryCount", out var value) ? (byte)(int)value : 0;
                if (currentRetryCount > retryCount)
                {
                    _logger.LogCritical("Retry count to handle message {MessageId} from dead letter queue reached limit", message.MessageId);
                    throw new InternalErrorException("Retry failed");
                }
                var cloned = message.Clone();
                cloned.UserProperties["CreyRetryCount"] = currentRetryCount + 1;
                await _senderClient.SendAsync(cloned);
                await _deadLetterQueue.CompleteAsync(message.SystemProperties.LockToken);
            }
            return messages.Count;
        }

        public async Task ConsumeMessageAsync(TMessageType message, IServiceProvider serviceProvider)
        {
            var context = new ValidationContext(message, serviceProvider: serviceProvider, items: null);
            Validator.ValidateObject(message, context, true);
            var handler = serviceProvider.GetRequiredService<IMessageHandler<TMessageType>>();

            using (servicebus_consume_message_duration_seconds.WithLabels(message.Type).NewTimer())
            {
                await handler.Handle(serviceProvider, message);
            }
        }

        private async Task HandleMessageAsync(Message message, CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            var payload = ReadMessage(message, scopedProvider);

            await ConsumeMessageAsync(payload, scopedProvider);
        }

        private TMessageType ReadMessage(Message message, IServiceProvider serviceProvider)
        {
            TMessageType payload;
            var bodyObject = JObject.Parse(Encoding.UTF8.GetString(message.Body));
            var rawPayload = bodyObject["Payload"];
            if (rawPayload.Type == JTokenType.String)
            {
                payload = _serializer.DeserializeMessage(rawPayload.ToString());
            }
            else if (rawPayload.Type == JTokenType.Object)
            {
                payload = _serializer.DeserializeMessage((JObject)rawPayload);
            }
            else
            {
                throw new InternalErrorException($"Unexpected payload: {rawPayload.ToString()}");
            }

            if (payload == null)
                throw new InternalErrorException($"Unknown SB payload");

            return payload;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            await HandleMessageAsync(message, token);
            await _receiverClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            var exception = exceptionReceivedEventArgs.Exception;
            _logger.LogWarning(exception, $"Message error received: {exception.Message}\n Context: {context}");

            await Task.CompletedTask;
        }

        private void InitializeClients(ChannelType channelType)
        {
            var channelName = _configuration.GetServiceBusChannelName(channelType, _channel);
            var retryPolicy = new RetryExponential(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), 10);
            var connectionString = _configuration.GetServiceBusConnectionString();
            switch (channelType)
            {
                case ChannelType.PublishSubscribe:
                    _receiverClient = new SubscriptionClient(connectionString, channelName, _subscriptionName, retryPolicy: retryPolicy);
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

            string deadPath = EntityNameHelper.FormatDeadLetterPath(_receiverClient.Path);
            _deadLetterQueue = new MessageReceiver(connectionString, deadPath, ReceiveMode.PeekLock);
        }

        [Obsolete("this function will be removed once the CodeFirstServiceBus feature is permanently turned on or at least move into InfrastructureUpdater")]
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
                var messageHandler = scopedProvider.GetRequiredService<IMessageHandler<TMessageType>>();

                var currentRules = await subscriptionClient.GetRulesAsync();

                if (currentRules.All(rule => ParseRuleName(rule) < messageHandler.CurrentVersion))
                {
                    // First we add a rule so new messages can qualify
                    var typeFilters = string.Join(",", messageHandler.HandledTypes.Select(type => $"'{type}'"));
                    var service = $"{_subscriptionName}-{_channel}";
                    var compiledFilterString = $"(To = '{service}' OR To IS NULL) AND (Type IN ({typeFilters}))";
                    await subscriptionClient.AddRuleAsync(
                        messageHandler.CurrentVersion.ToString(DateTimeFormat),
                        new SqlFilter(compiledFilterString));

                    // Remove each rule that are older than the one we just added
                    currentRules = await subscriptionClient.GetRulesAsync();
                    foreach (var rule in currentRules)
                    {
                        if (ParseRuleName(rule) < messageHandler.CurrentVersion)
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