namespace Crey.MessageContracts.Notification
{
    [MessageSerde("GameInvitation")]
    public sealed class GameInvitation : NotificationPayload
    {
        public int LevelId { get; set; }
        public int InviterUserId { get; set; }
    }

    [MessageSerde("GameInvitation")]
    public sealed class GameInvitationMessage : NotificationMessage<GameInvitation>
    {
    }
}
