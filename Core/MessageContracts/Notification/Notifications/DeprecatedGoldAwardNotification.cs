namespace Crey.MessageContracts.Notification
{
    [MessageSerde("GoldAward")]
    public sealed class DeprecatedGoldAwardNotification : NotificationPayload
    {
        public string AwardName { get; set; }
        public string ButtonText { get; set; } = "Collect";
        public int Gold { get; set; }

        public DeprecatedGoldAwardNotification()
        {
        }

        public DeprecatedGoldAwardNotification(string name, int amount)
        {
            AwardName = name;
            Gold = amount;
        }
    }

    [MessageSerde("GoldAward")]
    public sealed class DeprecatedGoldAwardNotificationMessage : NotificationMessage<DeprecatedGoldAwardNotification>
    {
    }
}
