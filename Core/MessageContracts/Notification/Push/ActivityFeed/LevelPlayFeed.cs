using System;

namespace Crey.MessageContracts.Notification.Push
{
    [Obsolete]
    public sealed class LevelPlayFeedPayload
    {
        public string Type => "Play";

        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
    }

    [MessageSerde("LevelPlayFeed")]
    public sealed class LevelPlayFeed : DeprecatedActivityFeedMessage<LevelPlayFeedPayload>
    {
        public int SenderUserId => Payload.SenderUserId;
        public int LevelId => Payload.LevelId;

        public LevelPlayFeed() { }

        public LevelPlayFeed(int senderUserId, int levelId)
            : base(new LevelPlayFeedPayload { SenderUserId = senderUserId, LevelId = levelId })
        {
        }
    }
}
