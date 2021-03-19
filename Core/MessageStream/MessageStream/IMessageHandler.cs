using Crey.MessageContracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.MessageStream
{
    public interface IMessageHandler<TMessageType>
        where TMessageType : IMessageContract
    {
        DateTime CurrentVersion { get; }
        IEnumerable<string> HandledTypes { get; }

        Task Handle(IServiceProvider serviceProvider, TMessageType message);
    }

    public static class IMessageHandlerExtension
    {
        public static IServiceCollection AddMessageHandler<TMessageType>(this IServiceCollection collectionBuilder, IMessageHandler<TMessageType> handler)
            where TMessageType : IMessageContract
        {
            return collectionBuilder.AddSingleton(handler);
        }

        public static bool HasMessageHandler<TMessageType>(this IServiceCollection collectionBuilder)
            where TMessageType : class, IMessageContract
        {
            return collectionBuilder.Any(d => d.ServiceType == typeof(IMessageHandler<TMessageType>));
        }

        public static IServiceCollection AssertMessageHandler<TMessageType>(this IServiceCollection collectionBuilder)
            where TMessageType : class, IMessageContract
        {
            if (!collectionBuilder.HasMessageHandler<TMessageType>())
                throw new InternalErrorException($"Message handler was not registered: {typeof(TMessageType).Name}");
            return collectionBuilder;
        }
    }
}
