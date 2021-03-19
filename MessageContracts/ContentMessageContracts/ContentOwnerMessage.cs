namespace ContentMessageContracts
{
    /// <summary>
    /// Message sent for the owner of the content (ex the one who created/owns the game)
    /// </summary>
    public abstract class ContentOwnerMessage : IContentMessage
    {
        public abstract string Type { get; }

        //once SenderUserId was removed, it can be made into a non-abstract member
        public abstract int OwnerId { get; set; }
    }
}
