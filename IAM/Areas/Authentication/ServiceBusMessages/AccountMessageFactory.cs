
using Crey.MessageStream;
using System;
using System.Linq;

namespace IAM.Areas.Authentication.ServiceBusMessages
{
    public class AccountMessageFactory : MessageFactory<IAccountServiceBusMessage>
    {
        public override DateTime CurrentVersion => new DateTime(2020, 9, 18);

        public AccountMessageFactory()
            : base(new[]{
                    Register<RegisteredServiceBusMessage>(),
                    Register<SoftDeleteServiceBusMessage>(),
                    Register<CancelSoftDeleteServiceBusMessage>(),
                    Register<HardDeleteServiceBusMessage>(),
                }.ToDictionary(x => x.Key, x => x.Value))
        {
        }
    }
}