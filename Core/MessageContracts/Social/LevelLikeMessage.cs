namespace Crey.MessageContracts.Social
{
    [MessageSerde("LevelLike")]
    public sealed class LevelLikeMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public int LevelId { get; set; }
        public long LevelLikeCount { get; set; }
        public int TotalLevelLikeCount { get; set; }
        public bool IsLike { get; set; }

        public LevelLikeMessage()
        {
        }
    }
}
