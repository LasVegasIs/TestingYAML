#nullable enable
namespace Crey.MessageContracts.Account
{
    [MessageSerde("RegisteredUserAccount")]
    public sealed class RegisteredUserAccount : AccountMessage
    {
        public string? CreyTicket { get; set; }
    }
}