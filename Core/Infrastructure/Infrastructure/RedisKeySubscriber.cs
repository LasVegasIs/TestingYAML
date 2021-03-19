using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crey.Infrastructure
{
    public interface IRedisKeySubscriptionHandler
    {
        Task Handle(string _notificationType, IServiceProvider _serviceProvider);
    }

    public class RedisKeySubscriberOptions
    {
        public string ConnectionString { get; set; } = "Please set a connection string";
    };

    public class RedisKeySubscriber
    {
        public RedisKeySubscriberOptions Options;
        private readonly Dictionary<string, IList<IRedisKeySubscriptionHandler>> _typeMap;

        public RedisKeySubscriber(Action<RedisKeySubscriberOptions> optionsBuilder, IServiceProvider serviceProvider)
        {
            _typeMap = new Dictionary<string, IList<IRedisKeySubscriptionHandler>>();

            Options = new RedisKeySubscriberOptions();
            optionsBuilder(Options);

            var subscriber = ConnectionMultiplexer.Connect(Options.ConnectionString).GetSubscriber();

            int db = 0; //what Redis DB do you want notifications on?
            string notificationChannel = $"__keyspace@{db}__:*";
            subscriber.Subscribe(notificationChannel, (channel, notificationType) =>
            {
                var key = GetKey(channel);
                var keyHandlers = _typeMap[key];
                foreach (var handler in keyHandlers)
                {
                    handler.Handle(notificationType, serviceProvider);
                }
            });
        }

        public void RegisterHandlerToKey<T>(string key)
            where T : IRedisKeySubscriptionHandler, new()
        {
            IList<IRedisKeySubscriptionHandler>? handlers = null;
            if (!_typeMap.TryGetValue(key, out handlers))
            {
                handlers = new List<IRedisKeySubscriptionHandler>();
                _typeMap.Add(key, handlers);
            }

            handlers.Add(new T());
        }

        private string GetKey(string channel)
        {
            var index = channel.IndexOf(':');
            if (index >= 0 && index < channel.Length - 1)
                return channel.Substring(index + 1);

            //we didn't find the delimeter, so just return the whole thing
            return channel;
        }
    }

    public static class RedisKeySubscriberExtensions
    {
        public static IServiceCollection AddRedisKeySubscriber(this IServiceCollection collection, Action<RedisKeySubscriberOptions> optionsBuilder)
        {
            collection.AddSingleton((services) => new RedisKeySubscriber(optionsBuilder, services));
            return collection;
        }

        public static IApplicationBuilder RegisterRedisKeySubscriptionHandler<T>(this IApplicationBuilder applicationBuilder, string key)
            where T : IRedisKeySubscriptionHandler, new()
        {
            var redisKeySubscriber = applicationBuilder.ApplicationServices.GetRequiredService<RedisKeySubscriber>();
            redisKeySubscriber.RegisterHandlerToKey<T>(key);

            return applicationBuilder;
        }
    }
}
