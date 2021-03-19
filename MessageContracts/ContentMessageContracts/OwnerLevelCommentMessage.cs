namespace ContentMessageContracts
{
    public class OwnerLevelCommentMessage : ContentOwnerMessage
    {
        public override string Type => "OwnerLevelCommentMessage";

        public override int OwnerId { get; set; }
        public int LevelId { get; set; }
        public ulong CountOnLevel { get; set; }
        public ulong CountOnAllLevels { get; set; }

        public OwnerLevelCommentMessage()
        {
        }

        public OwnerLevelCommentMessage(int ownerId, int levelId, ulong countOnLevel, ulong countOnAllLevels)
        {
            LevelId = levelId;
            OwnerId = ownerId;
            CountOnLevel = countOnLevel;
            CountOnAllLevels = countOnAllLevels;
        }
    }

}
