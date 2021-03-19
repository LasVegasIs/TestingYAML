namespace Crey.MessageContracts.Notification
{
    [MessageSerde("PackAward")]
    public sealed class PackAward : NotificationPayload
    {
        public string AwardName { get; set; }
        public string ButtonText { get; set; } = "Collect";
        public int Pack { get; set; }
    }

    [MessageSerde("PackAward")]
    public sealed class PackAwardMessage : NotificationMessage<PackAward>
    {
    }
}
