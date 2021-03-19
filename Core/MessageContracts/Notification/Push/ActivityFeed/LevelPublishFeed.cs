using System;

namespace Crey.MessageContracts.Notification.Push
{
    [Obsolete]
    public sealed class LevelPublishFeedPayload
    {
        public string Type => "Publish";

        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
    }

    [MessageSerde("LevelPublishFeed")]
    public sealed class LevelPublishFeed : DeprecatedActivityFeedMessage<LevelPublishFeedPayload>
    {
        public int SenderUserId => Payload.SenderUserId;
        public int LevelId => Payload.LevelId;

        public LevelPublishFeed() { }

        public LevelPublishFeed(int senderUserId, int levelId)
            : base(new LevelPublishFeedPayload { SenderUserId = senderUserId, LevelId = levelId })
        {
        }
    }
}
