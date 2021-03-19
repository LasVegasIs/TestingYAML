using System;
using System.Threading.Tasks;

namespace Crey.MessageStream.ServiceBus
{
    public interface ISentServiceBusMessage : IStreamedMessage
    {
    }

    public interface IReceivedServiceBusMessage : IStreamedMessage
    {
        Task Handle(IServiceProvider serviceProvider);
    }
}
