using Crey.Configuration.ConfigurationExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Crey.MessageStream.ServiceBus
{
    public static class ServiceBusExtensions
    {
        private const string DateTimeFormat = "yyyy.MM.dd";

        /// <summary>
        /// For sending messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusTopicBrokerAsync<TPayload>(this IServiceCollection collectionBuilder, IConfiguration configuration)
            where TPayload : class, ISentServiceBusMessage
        {
            var attribute = typeof(TPayload).GetCustomAttributes(typeof(ServiceBusTopicAttribute), true).SingleOrDefault() as ServiceBusTopicAttribute;
            if (attribute == null)
            {
                throw new Exception($"Missing topic, use {nameof(ServiceBusTopicAttribute)}");
            }

            if (configuration.GetValue<bool>("CodeFirstServiceBus", false))
            {
                await CreateTopicAsync(configuration, attribute.Topic);
            }

            collectionBuilder.AddScoped(svc => new CreyMessageBroker<TPayload>(attribute.Topic, svc, ChannelType.PublishSubscribe));
            return collectionBuilder;
        }

        /// <summary>
        /// For receiving messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusTopicSubscriberAsync<TMessageFactory, TPayload>(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration,
            string serviceName)
            where TPayload : class, IReceivedServiceBusMessage
            where TMessageFactory : class, IMessageFactory<TPayload>, new()
        {
            var attribute = typeof(TPayload).GetCustomAttributes(typeof(ServiceBusTopicAttribute), true).SingleOrDefault() as ServiceBusTopicAttribute;
            if (attribute == null)
            {
                throw new Exception($"Missing topic, use {nameof(ServiceBusTopicAttribute)}");
            }

            if (configuration.GetValue<bool>("CodeFirstServiceBus", false))
            {
                var messageFactory = new TMessageFactory();
                collectionBuilder.AddSingleton(messageFactory);
                var connectionString = configuration.GetServiceBusConnectionString();
                string channelName = configuration.GetChannelName(attribute.Topic);
                await CreateOrUpdateSubscriptionAsync<TPayload>(connectionString, channelName, serviceName, attribute.Topic, messageFactory);
                collectionBuilder.AddHealthChecks().AddAzureServiceBusTopic(connectionString, attribute.Topic);
            }

            collectionBuilder.AddSingleton(svc => new MessageSubscriber<TMessageFactory, TPayload>(serviceName, attribute.Topic, svc, ChannelType.PublishSubscribe));
            return collectionBuilder;
        }

        /// <summary>
        /// For sending messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusQueueBrokerAsync<TPayload>(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration)
            where TPayload : class, ISentServiceBusMessage
        {
            var attribute = typeof(TPayload).GetCustomAttributes(typeof(ServiceBusQueueAttribute), true).SingleOrDefault() as ServiceBusQueueAttribute;
            if (attribute == null)
            {
                throw new Exception($"Missing queue, use {nameof(ServiceBusQueueAttribute)}");
            }

            await CreateQueueAsync(configuration, attribute.Queue);

            collectionBuilder.AddScoped(svc => new CreyMessageBroker<TPayload>(attribute.Queue, svc, ChannelType.PointToPoint));
            return collectionBuilder;
        }

        /// <summary>
        /// For receiving messages.
        /// </summary>
        public static IServiceCollection AddServiceBusQueueSubscriber<TMessageFactory, TPayload>(
            this IServiceCollection collectionBuilder,
            string serviceName)
            where TPayload : class, IReceivedServiceBusMessage
            where TMessageFactory : class, IMessageFactory<TPayload>, new()
        {
            var attribute = typeof(TPayload).GetCustomAttributes(typeof(ServiceBusQueueAttribute), true).SingleOrDefault() as ServiceBusQueueAttribute;
            if (attribute == null)
            {
                throw new Exception($"Missing queue, use {nameof(ServiceBusQueueAttribute)}");
            }

            var messageFactory = new TMessageFactory();
            collectionBuilder.AddSingleton(messageFactory);

            collectionBuilder.AddSingleton(svc => new MessageSubscriber<TMessageFactory, TPayload>(serviceName, attribute.Queue, svc, ChannelType.PointToPoint));
            return collectionBuilder;
        }

        /// <summary>
        /// For sending and receiving messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusQueueBrokerAndSubscriberAsync<TMessageFactory, TPayload>(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration,
            string serviceName)
            where TPayload : class, ISentServiceBusMessage, IReceivedServiceBusMessage
            where TMessageFactory : class, IMessageFactory<TPayload>, new()
        {
            var attribute = typeof(TPayload).GetCustomAttributes(typeof(ServiceBusQueueAttribute), true).SingleOrDefault() as ServiceBusQueueAttribute;
            if (attribute == null)
            {
                throw new Exception($"Missing queue, use {nameof(ServiceBusQueueAttribute)}");
            }

            await CreateQueueAsync(configuration, attribute.Queue);

            var messageFactory = new TMessageFactory();
            collectionBuilder.AddSingleton(messageFactory);

            collectionBuilder.AddScoped(svc => new CreyMessageBroker<TPayload>(attribute.Queue, svc, ChannelType.PointToPoint));
            collectionBuilder.AddSingleton(svc => new MessageSubscriber<TMessageFactory, TPayload>(serviceName, attribute.Queue, svc, ChannelType.PointToPoint));
            return collectionBuilder;
        }

        /// <summary>
        /// Attach to service bus.
        /// </summary>
        public static async Task<IApplicationBuilder> RegisterMessageHandler<TMessageFactory, TPayload>(this IApplicationBuilder app)
            where TPayload : class, IReceivedServiceBusMessage
            where TMessageFactory : class, IMessageFactory<TPayload>
        {
            var bus = app.ApplicationServices.GetService<MessageSubscriber<TMessageFactory, TPayload>>();
            await bus.RegisterSubscriber();
            return app;
        }

        public static string GetServiceBusConnectionString(this IConfiguration configuration)
        {
            return configuration.GetValue<string>($"{configuration.GetDeploymentSlot()}-creybus");
        }

        private static Task CreateQueueAsync(IConfiguration configuration, string queuePath)
        {
            var connectionString = configuration.GetServiceBusConnectionString();
            var channelName = configuration.GetChannelName(queuePath);

            var managementClient = new ManagementClient(connectionString);
            return managementClient.CreateQueueAsync(channelName).IgnoreRaceConditionException<MessagingEntityAlreadyExistsException>();
        }

        private static Task CreateTopicAsync(IConfiguration configuration, string topicPath)
        {
            var connectionString = configuration.GetServiceBusConnectionString();
            var channelName = configuration.GetChannelName(topicPath);

            var managementClient = new ManagementClient(connectionString);
            return managementClient.CreateTopicAsync(channelName).IgnoreRaceConditionException<MessagingEntityAlreadyExistsException>();
        }

        private static async Task CreateOrUpdateSubscriptionAsync<TPayload>(
            string connectionString,
            string channelName,
            string subscriptionName,
            string topicPath,
            IMessageFactory<TPayload> messageFactory)
            where TPayload : class, IReceivedServiceBusMessage
        {
            var managementClient = new ManagementClient(connectionString);
            var ruleDescription = GetRuleDescriptionFromTypeMap(subscriptionName, topicPath, messageFactory);

            try
            {
                var subscriptionDescription = new SubscriptionDescription(channelName, subscriptionName);
                await managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                await UpdateSubscriptionRules(managementClient, channelName, subscriptionName, ruleDescription, messageFactory);
            }
        }

        private static async Task UpdateSubscriptionRules<TPayload>(
            ManagementClient managementClient,
            string channelName,
            string subscriptionName,
            RuleDescription ruleDescription,
            IMessageFactory<TPayload> messageFactory)
            where TPayload : class, IReceivedServiceBusMessage
        {
            var currentRules = await managementClient.GetRulesAsync(channelName, subscriptionName);
            if (currentRules.All(rule => ParseRuleName(rule) < messageFactory.CurrentVersion))
            {
                // First we add a rule so new messages can qualify
                await managementClient.CreateRuleAsync(channelName, subscriptionName, ruleDescription)
                    .IgnoreRaceConditionException<MessagingEntityAlreadyExistsException>();

                // Remove each rule that are older than the one we just added
                currentRules = await managementClient.GetRulesAsync(channelName, subscriptionName);
                foreach (var rule in currentRules)
                {
                    if (ParseRuleName(rule) < messageFactory.CurrentVersion)
                    {
                        await managementClient.DeleteRuleAsync(channelName, subscriptionName, rule.Name)
                            .IgnoreRaceConditionException<MessagingEntityNotFoundException>();
                    }
                }
            }
        }

        private static RuleDescription GetRuleDescriptionFromTypeMap<TPayload>(
            string subscriptionName,
            string topicPath,
            IMessageFactory<TPayload> messageFactory)
            where TPayload : class, IReceivedServiceBusMessage
        {
            var service = $"{subscriptionName}-{topicPath}";
            var typeFilters = string.Join(",", messageFactory.TypeMap.Keys.Select(type => $"'{type}'"));
            var compiledFilterString = $"(To = '{service}' OR To IS NULL) AND (Type IN ({typeFilters}))";

            return new RuleDescription(
                messageFactory.CurrentVersion.ToString(DateTimeFormat),
                new SqlFilter(compiledFilterString));
        }

        private static DateTime ParseRuleName(RuleDescription rule)
        {
            return DateTime.TryParseExact(rule.Name, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)
                ? result
                : DateTime.MinValue;
        }

        private static async Task IgnoreRaceConditionException<TException>(this Task task)
            where TException : ServiceBusException
        {
            try
            {
                await task;
            }
            catch (TException)
            {
                // noop, another instance got here first
            }
        }
    }
}
