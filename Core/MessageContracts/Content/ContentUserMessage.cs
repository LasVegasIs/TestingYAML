namespace Crey.MessageContracts.Content
{
    /// <summary>
    /// Message sent for the user of a content (ex the one who plays with the game)
    /// </summary>
    public abstract class ContentUserMessage : ContentMessage
    {
        //once SenderUserId was removed, it can be made into a non-abstract member
        public abstract int UserId { get; set; }
    }
}
