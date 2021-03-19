using Crey.MessageContracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.MessageStream.InMemoryQueue
{
#nullable enable
    public class InMemoryMessageQueue<TMessageType> : IMessageProducer<TMessageType>
        where TMessageType : IMessageContract
    {
        private ConcurrentQueue<TMessageType> _messages = new ConcurrentQueue<TMessageType>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public Task SendMessageAsync(TMessageType message)
        {
            _messages.Enqueue(message);
            _signal.Release();

            return Task.CompletedTask;
        }

        public Task SendMessagesAsync(IEnumerable<TMessageType> messages)
        {
            foreach (var message in messages)
            {
                _messages.Enqueue(message);
            }
            _signal.Release(messages.Count());

            return Task.CompletedTask;
        }

        public async Task<TMessageType?> ReceiveMessageAsync()
        {
            await _signal.WaitAsync();
            _messages.TryDequeue(out var message);
            return message;
        }
    }
#nullable restore
}