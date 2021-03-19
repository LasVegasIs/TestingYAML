#nullable enable
namespace AccountMessageContracts
{
    public class RegisteredUserAccount : AccountMessage
    {
        public override string Type => nameof(RegisteredUserAccount);

        public string? CreyTicket { get; set; }
    }
}
#nullable restore