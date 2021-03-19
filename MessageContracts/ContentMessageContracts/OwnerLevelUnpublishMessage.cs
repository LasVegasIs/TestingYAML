﻿namespace ContentMessageContracts
{
    public class OwnerLevelUnpublishMessage : ContentOwnerMessage
    {
        public override string Type => "OwnerLevelUnpublishMessage";

        public override int OwnerId { get; set; }
        public int LevelId { get; set; }
        public ulong PublishedLevelCount { get; set; }

        public OwnerLevelUnpublishMessage()
        {
        }

        public OwnerLevelUnpublishMessage(int ownerId, int levelId, ulong publishedLevelCount)
        {
            OwnerId = ownerId;
            LevelId = levelId;
            PublishedLevelCount = publishedLevelCount;
        }
    }
}
