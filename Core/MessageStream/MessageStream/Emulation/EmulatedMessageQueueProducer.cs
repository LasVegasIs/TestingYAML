using Crey.MessageContracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.MessageStream.Emulation
{
#nullable enable
    public class EmulatedMessageQueueProducer<TMessageType> : IMessageProducer<TMessageType>
        where TMessageType : IMessageContract
    {
        private readonly MessageSerializer<TMessageType> _serializer;

        public EmulatedMessageQueueProducer(MessageSerializer<TMessageType> serializer)
        {
            _serializer = serializer;
        }

        public Task SendMessageAsync(TMessageType message)
        {
            var msg = _serializer.Serialize(message);
            System.Console.Error.WriteLine($"---------------\n{msg}\n##############");
            return Task.CompletedTask;
        }

        public async Task SendMessagesAsync(IEnumerable<TMessageType> messages)
        {
            foreach (var message in messages)
            {
                await SendMessageAsync(message);
            }
        }
    }

    public static class EmulatedMessageQueueProducerExtensions
    {
        public static IServiceCollection AddEmulatedMessageQueueProducer<TMessageType>(this IServiceCollection services)
            where TMessageType : class, IMessageContract
        {
            services.TryAddMessageSerializer<TMessageType>();
            services.AddSingleton<IMessageProducer<TMessageType>, EmulatedMessageQueueProducer<TMessageType>>();
            return services;
        }
    }
}