namespace Crey.MessageContracts.Notification
{
    [MessageSerde("GameEventLevelUp")]
    public sealed class GameEventLevelUp : RewardTrigger
    {
        public long EventId { get; set; }

        public GameEventLevelUp() {}

        public GameEventLevelUp(long eventId)
        {
            EventId = eventId;
        }
    }
}