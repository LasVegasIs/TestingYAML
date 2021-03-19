namespace UserProfileMessageContracts
{
    public class UserXPChanged : UserProfileMessage
    {
        public override string Type => "UserXPChanged";

        public ulong OldXPValue { get; set; }
        public ulong NewXPValue { get; set; }
    }
}
