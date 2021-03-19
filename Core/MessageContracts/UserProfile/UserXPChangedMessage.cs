namespace Crey.MessageContracts.UserProfile
{
    [MessageSerde("UserXPChanged")]
    public class UserXPChangedMessage : UserProfileMessage
    {
        public ulong OldXPValue { get; set; }
        public ulong NewXPValue { get; set; }
    }
}
