
using MessagingCore;

namespace AccountMessageContracts
{
    public interface IAccountMessage: IMessageContract
    {
        public const string TOPIC = "user-account";
    }

    public abstract class AccountMessage : IAccountMessage
    {
        public abstract string Type { get; }

        public int AccountId { get; set; }
    }
}
