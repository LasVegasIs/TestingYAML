namespace Crey.MessageContracts.Notification.Push
{
    [MessageSerde("UserXP")]
    public sealed class UserXPChangedFeed : UserStatusFeedMessage
    {
        public ulong OldXPValue { get; set; }
        public ulong NewXPValue { get; set; }

        public UserXPChangedFeed() { }

        public UserXPChangedFeed(ulong oldXPValue, ulong newXPValue)
        {
            OldXPValue = oldXPValue;
            NewXPValue = newXPValue;
        }
    }
}
