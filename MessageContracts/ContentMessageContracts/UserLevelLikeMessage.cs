namespace ContentMessageContracts
{
    public class UserLevelLikeMessage : ContentUserMessage
    {
        public override string Type => "UserLevelLikeMessage";

        public override int UserId { get; set; }
        public int LevelId { get; set; }
        public LikeType LikeType { get; set; }
        public ulong CountOnAllLevels { get; set; }

        public UserLevelLikeMessage()
        {
        }

        public UserLevelLikeMessage(int userId, int levelId, LikeType likeType, ulong countOnAllLevels)
        {
            UserId = userId;
            LevelId = levelId;
            LikeType = likeType;
            CountOnAllLevels = countOnAllLevels;
        }
    }
}
