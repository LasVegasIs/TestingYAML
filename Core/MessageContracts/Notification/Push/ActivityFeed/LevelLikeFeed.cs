using System;

namespace Crey.MessageContracts.Notification.Push
{
    [Obsolete]
    public sealed class LevelLikeFeedPayload
    {
        public string Type => "LevelLike";

        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
    }

    [MessageSerde("LevelLikeFeed")]
    public sealed class LevelLikeFeed : DeprecatedActivityFeedMessage<LevelLikeFeedPayload>
    {
        public int SenderUserId => Payload.SenderUserId;
        public int LevelId => Payload.LevelId;

        public LevelLikeFeed() { }

        public LevelLikeFeed(int senderUserId, int levelId)
            : base(new LevelLikeFeedPayload { SenderUserId = senderUserId, LevelId = levelId })
        {
        }
    }
}
