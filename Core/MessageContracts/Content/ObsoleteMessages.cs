using System;

namespace Crey.MessageContracts.Content
{
    [MessageSerde("Play")]
    public sealed class PlayMessage : ContentMessage
    {
        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
        public int OwnerId { get; set; }
        [Obsolete]
        public int OwnerTotalPlayCount { get; set; }
        public long LevelPlayCount { get; set; }
    }

    [MessageSerde("Unpublish")]
    public sealed class UnpublishMessage : ContentMessage
    {

        public int SenderUserId { get; set; }

        public int LevelId { get; set; }
    }

    [MessageSerde("Finish")]
    public sealed class LevelFinishedMessage : ContentMessage
    {
        public int SenderUserId { get; set; }

        public int LevelId { get; set; }
    }
}
