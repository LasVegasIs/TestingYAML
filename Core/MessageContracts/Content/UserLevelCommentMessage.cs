namespace Crey.MessageContracts.Content
{
    [MessageSerde("UserLevelCommentMessage")]
    public class UserLevelCommentMessage : ContentUserMessage
    {
        public override int UserId { get; set; }
        public int LevelId { get; set; }
        public ulong CountOnAllLevels { get; set; }

        public UserLevelCommentMessage()
        {
        }

        public UserLevelCommentMessage(int userId, int levelId, ulong countOnAllLevels)
        {
            UserId = userId;
            LevelId = levelId;
            CountOnAllLevels = countOnAllLevels;
        }
    }

}
