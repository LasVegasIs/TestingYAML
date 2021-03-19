namespace Crey.MessageContracts.Social
{
    [MessageSerde("Comment")]
    public sealed class CommentMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public string CommentId { get; set; }
        public int LevelId { get; set; }

        public CommentMessage()
        {
        }
    }
}
