using Crey.MessageContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Core.MessageStream.ServiceBus
{
    public static class ServiceBusExtensions
    {
        /// <summary>
        /// For sending messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusTopicBrokerAsync<TMessageType>(this IServiceCollection collectionBuilder, IConfiguration configuration)
            where TMessageType : class, IMessageContract
        {
            var attribute = MessageTopicAttribute.GetRequiredForType<TMessageType>();

            if (configuration.IsCodeFirstServiceBus())
            {
                var infrastructureUpdater = new InfrastructureUpdater(configuration);
                await infrastructureUpdater.CreateTopicAsync(attribute.Topic);
            }

            collectionBuilder.TryAddMessageSerializer<TMessageType>();
            collectionBuilder.AddSingleton<IMessageProducer<TMessageType>>(svc => new MessageBroker<TMessageType>(attribute.Topic, svc, ChannelType.PublishSubscribe));
            return collectionBuilder;
        }

        /// <summary>
        /// For receiving messages.
        /// </summary>
        public static IServiceCollection AddServiceBusTopicSubscriberAsync<TMessageType>(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration,
            string serviceName)
            where TMessageType : class, IMessageContract
        {
            var attribute = MessageTopicAttribute.GetRequiredForType<TMessageType>();

            if (configuration.IsCodeFirstServiceBus())
            {
                var connectionString = configuration.GetServiceBusConnectionString();
                collectionBuilder.AddHealthChecks().AddAzureServiceBusTopic(connectionString, attribute.Topic);
            }

            collectionBuilder.TryAddMessageSerializer<TMessageType>();
            collectionBuilder.AssertMessageHandler<TMessageType>();
            collectionBuilder.AddSingleton<IMessageConsumer<TMessageType>>(svc => new MessageSubscriber<TMessageType>(serviceName, attribute.Topic, ChannelType.PublishSubscribe, svc));
            return collectionBuilder;
        }

        /// <summary>
        /// For sending messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusQueueBrokerAsync<TMessageType>(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration)
            where TMessageType : class, IMessageContract
        {
            var attribute = MessageTopicAttribute.GetRequiredForType<TMessageType>();

            var infrastructureUpdater = new InfrastructureUpdater(configuration);
            await infrastructureUpdater.CreateQueueAsync(attribute.Topic);

            collectionBuilder.TryAddMessageSerializer<TMessageType>();
            collectionBuilder.AddSingleton<IMessageProducer<TMessageType>>(svc => new MessageBroker<TMessageType>(attribute.Topic, svc, ChannelType.PointToPoint));
            return collectionBuilder;
        }

        /// <summary>
        /// For receiving messages.
        /// </summary>
        public static IServiceCollection AddServiceBusQueueSubscriber<TMessageType>(
            this IServiceCollection collectionBuilder,
            string serviceName)
            where TMessageType : class, IMessageContract
        {
            var attribute = MessageTopicAttribute.GetRequiredForType<TMessageType>();

            collectionBuilder.TryAddMessageSerializer<TMessageType>();
            collectionBuilder.AssertMessageHandler<TMessageType>();
            collectionBuilder.AddSingleton<IMessageConsumer<TMessageType>>(svc => new MessageSubscriber<TMessageType>(serviceName, attribute.Topic, ChannelType.PointToPoint, svc));
            return collectionBuilder;
        }

        /// <summary>
        /// For sending and receiving messages.
        /// </summary>
        public static async Task<IServiceCollection> AddServiceBusQueueBrokerAndSubscriberAsync<TMessageType>(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration,
            string serviceName)
            where TMessageType : class, IMessageContract
        {
            await collectionBuilder.AddServiceBusQueueBrokerAsync<TMessageType>(configuration);
            collectionBuilder.AddServiceBusQueueSubscriber<TMessageType>(serviceName);
            return collectionBuilder;
        }

        public static async Task<Microsoft.AspNetCore.Builder.IApplicationBuilder> CreateOrUpdateTopicSubscriptionAsync<TMessageType>(
            this Microsoft.AspNetCore.Builder.IApplicationBuilder app,
            string serviceName)
            where TMessageType : class, IMessageContract
        {
            var configuration = app.ApplicationServices.GetService<IConfiguration>();
            if (configuration.IsCodeFirstServiceBus())
            {
                var attribute = MessageTopicAttribute.GetForType<TMessageType>();
                if (attribute == null)
                {
                    throw new InternalErrorException($"Missing topic, use {nameof(MessageTopicAttribute)}");
                }
                var connectionString = configuration.GetServiceBusConnectionString();
                var channelName = configuration.GetServiceBusChannelName(ChannelType.PublishSubscribe, attribute.Topic);
                var messageHandler = app.ApplicationServices.GetService<IMessageHandler<TMessageType>>();

                var infrastructureUpdater = new InfrastructureUpdater(configuration);
                await infrastructureUpdater.CreateOrUpdateSubscriptionAsync<TMessageType>(connectionString, channelName, serviceName, attribute.Topic, messageHandler);
            }

            return app;
        }

#if NETCORE_3_1 || NET_5_0
        /// <summary>
        /// Attach to service bus.
        /// </summary>
        public static async Task<Microsoft.AspNetCore.Builder.IApplicationBuilder> RegisterMessageHandler<TMessageType>(
            this Microsoft.AspNetCore.Builder.IApplicationBuilder app,
            ChannelType channelType,
            string serviceName)
            where TMessageType : class, IMessageContract
        {
            if (channelType == ChannelType.PublishSubscribe)
            {
                await app.CreateOrUpdateTopicSubscriptionAsync<TMessageType>(serviceName);
            }

            var bus = (MessageSubscriber<TMessageType>)app.ApplicationServices.GetService<IMessageConsumer<TMessageType>>()
                ?? throw new InternalErrorException("Message stream for {typeof(TMessageType)} is not a service bus message");

            await bus.RegisterSubscriber(channelType);
            return app;
        }
#endif
    }
}
