namespace Crey.MessageContracts.Notification
{
    [MessageSerde("XPGain")]
    public sealed class XPGain : NotificationPayload
    {
        public uint XP { get; set; }
    }

    [MessageSerde("XPGain")]
    public sealed class XPGainMessage : NotificationMessage<XPGain>
    {
    }
}
