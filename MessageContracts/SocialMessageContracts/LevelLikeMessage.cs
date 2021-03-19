namespace SocialMessageContracts
{
    public class LevelLikeMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public int LevelId { get; set; }
        public long LevelLikeCount { get; set; }
        public int TotalLevelLikeCount { get; set; }
        public bool IsLike { get; set; }

        public LevelLikeMessage()
            : base("LevelLike")
        {
        }
    }
}
