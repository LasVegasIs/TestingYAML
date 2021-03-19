using Crey.MessageContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.MessageStream
{
    public interface IMessageProducer<TMessageType>
        where TMessageType : IMessageContract
    {
        Task SendMessageAsync(TMessageType message);
        Task SendMessagesAsync(IEnumerable<TMessageType> messages);
    }
}