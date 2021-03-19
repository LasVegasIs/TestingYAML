namespace ContentMessageContracts
{
    /// <summary>
    /// Message sent for the user of a content (ex the one who plays with the game)
    /// </summary>
    public abstract class ContentUserMessage : IContentMessage
    {
        public abstract string Type { get; }

        //once SenderUserId was removed, it can be made into a non-abstract member
        public abstract int UserId { get; set; }
    }
}
