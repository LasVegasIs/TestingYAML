using System;

namespace ContentMessageContracts
{
    public class PublishLevelMessage : ContentOwnerMessage
    {
        public override string Type => "Publish";

        public override int OwnerId { get; set; }
        public int LevelId { get; set; }
        public ulong PublishedLevelCount { get; set; }


        [Obsolete("only for backward compatibility")]
        public int SenderuserId => OwnerId;

        public PublishLevelMessage()
        {
        }

        public PublishLevelMessage(int ownerId, int levelId, ulong publishedLevelCount)
        {
            OwnerId = ownerId;
            LevelId = levelId;
            PublishedLevelCount = publishedLevelCount;
        }
    }
}
