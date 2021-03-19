using System;

namespace Crey.MessageContracts.Notification
{
    [MessageSerde("GameEventObjectiveCompleted")]
    public sealed class GameEventObjectiveCompleted : RewardTrigger
    {
        public long EventId { get; set; }
        public Guid ObjectiveId { get; set; }

        internal GameEventObjectiveCompleted() { }

        public GameEventObjectiveCompleted(long eventId, Guid objectiveId)
        {
            ObjectiveId = objectiveId;
            EventId = eventId;
        }
    }
}