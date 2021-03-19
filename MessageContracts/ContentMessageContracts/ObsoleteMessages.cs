using System;

namespace ContentMessageContracts
{
    public class PlayMessage : ContentMessage
    {
        public int LevelId { get; set; }
        public int OwnerId { get; set; }
        [Obsolete]
        public int OwnerTotalPlayCount { get; set; }
        public long LevelPlayCount { get; set; }

        public PlayMessage()
            : base("Play")
        {
        }
    }

    public class UnpublishMessage : ContentMessage
    {
        public int LevelId { get; set; }

        public UnpublishMessage()
            : base("Unpublish")
        {
        }
    }

    public class LevelFinishedMessage : ContentMessage
    {
        public int LevelId { get; set; }

        public LevelFinishedMessage()
            : base("Finish")
        {
        }
    }
}
