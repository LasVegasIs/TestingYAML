using System;

namespace Crey.MessageContracts.Notification.Push
{
    [Obsolete]
    public sealed class LevelCommentFeedPayload
    {
        public string Type => "Comment";

        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
    }

    [MessageSerde("LevelCommentFeed")]
    public sealed class LevelCommentFeed : DeprecatedActivityFeedMessage<LevelCommentFeedPayload>
    {        
        public int SenderUserId => Payload.SenderUserId;
        public int LevelId => Payload.LevelId;

        public LevelCommentFeed() {}

        public LevelCommentFeed(int senderUserId, int levelId)
            : base(new LevelCommentFeedPayload { SenderUserId = senderUserId, LevelId = levelId })
        {
        }
    }
}
