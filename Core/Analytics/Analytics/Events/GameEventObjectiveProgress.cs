using Newtonsoft.Json;
using System;

namespace Analytics.Events
{
    public sealed class GameEventObjectiveProgress : AnalyticsEvent
    {
        public override string Event => "GameEvent_Objective_Progress";

        [JsonProperty("gamevent_id")]
        public long GameEventId { get; set; }

        [JsonProperty("objective_id")]
        public Guid ObjectiveId { get; set; }

        [JsonProperty("progress_value")]
        public ulong ProgressValue { get; set; }

        public GameEventObjectiveProgress(int userId, long gameEventId, Guid objectiveId, ulong progressValue)
            : base(userId)
        {
            GameEventId = gameEventId;
            ObjectiveId = objectiveId;
            ProgressValue = progressValue;
        }
    }
}
