namespace Crey.MessageContracts.Content
{
    [MessageSerde("UserLevelLikeMessage")]
    public class UserLevelLikeMessage : ContentUserMessage
    {
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
