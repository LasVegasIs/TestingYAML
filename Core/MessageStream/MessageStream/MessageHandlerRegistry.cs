using Crey.MessageContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.MessageStream
{
    public interface IRegisteredMessageHandler<TMessageType>
        where TMessageType : IMessageContract
    {
        string Type { get; }

        Task Handle(IServiceProvider _serviceProvider, TMessageType message);
    }

    /// <summary>
    ///  Make sure it stores no state, as handler is created only once and used many times.
    /// </summary>
    /// <typeparam name="TMessageType"></typeparam>
    public abstract class RegisteredMessageHandler<TMessageTypeBase, TMessageType> : IRegisteredMessageHandler<TMessageTypeBase>
        where TMessageTypeBase : IMessageContract
        where TMessageType : TMessageTypeBase, new()
    {
        public string Type => new TMessageType().Type;

        public Task Handle(IServiceProvider _serviceProvider, TMessageTypeBase message)
        {
            return HandleMessage(_serviceProvider, (TMessageType)message);
        }

        public abstract Task HandleMessage(IServiceProvider _serviceProvider, TMessageType message);
    }

    public abstract class MessageHandlerRegistry<TMessageType> : IMessageHandler<TMessageType>
        where TMessageType : IMessageContract
    {
        public abstract DateTime CurrentVersion { get; }
        public IEnumerable<string> HandledTypes => _typeMap.Keys;
        private readonly Dictionary<string, IRegisteredMessageHandler<TMessageType>> _typeMap;

        protected MessageHandlerRegistry()
        {
            _typeMap = new Dictionary<string, IRegisteredMessageHandler<TMessageType>>();
        }

        protected void Register<T>()
            where T : IRegisteredMessageHandler<TMessageType>, new()
        {
            var handler = new T();
            _typeMap.Add(handler.Type, handler);
        }

        public async Task Handle(IServiceProvider serviceProvider, TMessageType message)
        {
            if (_typeMap.TryGetValue(message.Type, out var handler))
            {
                await handler.Handle(serviceProvider, message);
            }
        }
    }
}
