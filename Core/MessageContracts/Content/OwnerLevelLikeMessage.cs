namespace Crey.MessageContracts.Content
{
    [MessageSerde("OwnerLevelLikeMessage")]
    public sealed class OwnerLevelLikeMessage : ContentOwnerMessage
    {
        public override int OwnerId { get; set; }
        public int LevelId { get; set; }
        public LikeType LikeType { get; set; }
        public uint CountOnLevel { get; set; }
        public ulong CountOnAllLevels { get; set; }

        public OwnerLevelLikeMessage()
        {
        }

        public OwnerLevelLikeMessage(int ownerId, int levelId, LikeType likeType, uint countOnLevel, ulong countOnAllLevels)
        {
            OwnerId = ownerId;
            LevelId = levelId;
            LikeType = likeType;
            CountOnLevel = countOnLevel;
            CountOnAllLevels = countOnAllLevels;
        }
    }
}
