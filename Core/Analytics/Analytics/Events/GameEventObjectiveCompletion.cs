using Newtonsoft.Json;
using System;

namespace Analytics.Events
{
    public sealed class GameEventObjectiveCompletion : AnalyticsEvent
    {
        public override string Event => "GameEvent_Objective_Completion";

        [JsonProperty("gamevent_id")]
        public long GameEventId { get; set; }

        [JsonProperty("objective_id")]
        public Guid ObjectiveId { get; set; }

        public GameEventObjectiveCompletion(int userId, long gameEventId, Guid objectiveId)
            : base(userId)
        {
            GameEventId = gameEventId;
            ObjectiveId = objectiveId;
        }
    }
}
