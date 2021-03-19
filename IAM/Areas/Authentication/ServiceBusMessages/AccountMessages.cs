using AccountMessageContracts;
using Crey.Kernel.ServiceDiscovery;
using Crey.MessageStream.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication.ServiceBusMessages
{
    [ServiceBusTopic(IAccountMessage.TOPIC)]
    public interface IAccountServiceBusMessage: IAccountMessage, ISentServiceBusMessage, IReceivedServiceBusMessage
    {
    }

    public class CancelSoftDeleteServiceBusMessage : CancelSoftDeleteUserAccount, IAccountServiceBusMessage
    {
        public Task Handle(IServiceProvider serviceProvider)
        {
            var accountRepository = serviceProvider.GetRequiredService<ICreyService<AccountRepository>>();
            return accountRepository.Value.CancelSoftDelete(AccountId);
        }
    }

    public class HardDeleteServiceBusMessage : HardDeleteUserAccount, IAccountServiceBusMessage
    {
        public Task Handle(IServiceProvider serviceProvider)
        {
            var accountRepository = serviceProvider.GetRequiredService<ICreyService<AccountRepository>>();
            return accountRepository.Value.HardDeleteAccount(AccountId);
        }
    }

    public class SoftDeleteServiceBusMessage : SoftDeleteUserAccount, IAccountServiceBusMessage
    {
        public Task Handle(IServiceProvider serviceProvider)
        {
            var accountRepository = serviceProvider.GetRequiredService<ICreyService<AccountRepository>>();
            return accountRepository.Value.SoftDeleteAccount(AccountId);
        }
    }

    public class RegisteredServiceBusMessage : RegisteredUserAccount, IAccountServiceBusMessage {
        public Task Handle(IServiceProvider serviceProvider) {
            var service = serviceProvider.GetService<AfterRegistrationHandler>();
            return service.AfterRegistration(this.AccountId, this.CreyTicket);
        }
    }
}