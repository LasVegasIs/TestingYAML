using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Crey.MessageContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.MessageStream.InMemoryQueue
{
#nullable enable
    public class InMemoryMessageQueueConsumerHostedService<TMessageType> : BackgroundService, IMessageConsumer<TMessageType>
        where TMessageType : IMessageContract
    {
        private readonly InMemoryMessageQueue<TMessageType> _messageQueue;
        private readonly ILogger<InMemoryMessageQueueConsumerHostedService<TMessageType>> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public InMemoryMessageQueueConsumerHostedService(
            InMemoryMessageQueue<TMessageType> messageQueue,
            ILogger<InMemoryMessageQueueConsumerHostedService<TMessageType>> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _messageQueue = messageQueue;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await BackgroundProcessing(stoppingToken);
        }

        public Task ConsumeMessageAsync(TMessageType message, IServiceProvider serviceProvider)
        {
            var context = new ValidationContext(message, serviceProvider: serviceProvider, items: null);
            Validator.ValidateObject(message, context, true);
            var handler = serviceProvider.GetRequiredService<IMessageHandler<TMessageType>>();
            return handler.Handle(serviceProvider, message);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _messageQueue.ReceiveMessageAsync();
                if (message == null)
                    continue;

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        await ConsumeMessageAsync(message, scope.ServiceProvider);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred consuming {Message}.", nameof(message));
                }
            }
        }
    }

    public static class InMemoryMessageQueueConsumerExtensions
    {
        public static IServiceCollection AddInMemoryMessageQueueConsumer<TMessageType>(this IServiceCollection services)
            where TMessageType : class, IMessageContract
        {
            services.AddHostedService<InMemoryMessageQueueConsumerHostedService<TMessageType>>();
            services.AddSingleton<IMessageProducer<TMessageType>, InMemoryMessageQueue<TMessageType>>();
            return services;
        }
    }
#nullable restore
}
