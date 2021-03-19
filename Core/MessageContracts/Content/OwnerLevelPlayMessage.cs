using System;

namespace Crey.MessageContracts.Content
{
    [MessageSerde("LevelPlay")]
    public sealed class OwnerLevelPlayMessage : ContentOwnerMessage
    {
        public override int OwnerId { get; set; }
        public int LevelId { get; set; }
        public ulong CountOnLevel { get; set; }
        public ulong CountOnAllLevels { get; set; }

        [Obsolete("only for backward compatibility")]
        public ulong TotalPlayCount => CountOnLevel;
        [Obsolete("only for backward compatibility")]
        public ulong TotalOwnedLevelsPlayCount => CountOnAllLevels;

        public OwnerLevelPlayMessage()
        {
        }

        public OwnerLevelPlayMessage(int ownerId, int levelId, ulong countOnLevel, ulong countOnAllLevels)
        {
            OwnerId = ownerId;
            LevelId = levelId;
            CountOnLevel = countOnLevel;
            CountOnAllLevels = countOnAllLevels;
        }
    }
}
