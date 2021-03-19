using Crey.MessageContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Core.MessageStream.Emulation
{
#nullable enable
    public class EmulatedMessageQueueConsumerOptions<TMessageType>
        where TMessageType : IMessageContract
    {
        public int Port { get; internal set; }
    }

    public class EmulatedMessageQueueConsumer<TMessageType> : BackgroundService, IMessageConsumer<TMessageType>
        where TMessageType : IMessageContract
    {
        private readonly EmulatedMessageQueueConsumerOptions<TMessageType> _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly HttpListener _listener;
        private readonly MessageSerializer<TMessageType> _serializer;

        public EmulatedMessageQueueConsumer(
            IServiceScopeFactory serviceScopeFactory,
            MessageSerializer<TMessageType> serializer,
            EmulatedMessageQueueConsumerOptions<TMessageType> options)
        {
            _options = options;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{_options.Port}/");
            _listener.Start();
            _serviceScopeFactory = serviceScopeFactory;
            _serializer = serializer;
        }

        public async Task ConsumeMessageAsync(TMessageType message, IServiceProvider serviceProvider)
        {
            var context = new ValidationContext(message, serviceProvider: serviceProvider, items: null);
            Validator.ValidateObject(message, context, true);
            var handler = serviceProvider.GetRequiredService<IMessageHandler<TMessageType>>();
            if (handler.HandledTypes.Contains(message.Type))
                await handler.Handle(serviceProvider, message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var ctx = await _listener.GetContextAsync();
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var serviceProvider = scope.ServiceProvider;
                        string json = await new StreamReader(ctx.Request.InputStream).ReadToEndAsync();
                        var message = _serializer.DeserializeMessage(json)
                            ?? throw new InternalErrorException($"Failed to parse message");

                        await ConsumeMessageAsync(message, serviceProvider);

                        ctx.Response.StatusCode = 200;
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes("ok");
                        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Error occurred consuming {ex.Message}";
                    ctx.Response.StatusCode = 500;
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(error);
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }

                ctx.Response.OutputStream.Close();
            }
        }
    }

    public static class EmulatedMessageQueueConsumerExtensions
    {
        public static IServiceCollection AddEmulatedMessageQueueConsumer<TMessageType>(this IServiceCollection services, int port)
            where TMessageType : class, IMessageContract
        {
            services.TryAddMessageSerializer<TMessageType>();
            services.AddSingleton(new EmulatedMessageQueueConsumerOptions<TMessageType> { Port = port });
            services.AddSingleton<EmulatedMessageQueueConsumer<TMessageType>>();
            services.AddSingleton<IMessageConsumer<TMessageType>>(svc => svc.GetRequiredService<EmulatedMessageQueueConsumer<TMessageType>>());
            services.AddHostedService(svc => svc.GetRequiredService<EmulatedMessageQueueConsumer<TMessageType>>());
            return services;
        }
    }
}
