using Crey.MessageContracts;
using System;
using System.Threading.Tasks;

namespace Core.MessageStream
{
    public interface IMessageConsumer<TMessageType>
        where TMessageType : IMessageContract
    {
        /// To inject messages externally, usually not required and exists only for debug
        Task ConsumeMessageAsync(TMessageType message, IServiceProvider serviceProvider);
    }
}