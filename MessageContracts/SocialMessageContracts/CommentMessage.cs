namespace SocialMessageContracts
{
    public class CommentMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public string CommentId { get; set; }
        public int LevelId { get; set; }

        public CommentMessage()
            : base("Comment")
        {
        }
    }
}
