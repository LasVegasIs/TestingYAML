namespace Crey.MessageContracts.Content
{
    /// <summary>
    /// Message sent for the owner of the content (ex the one who created/owns the game)
    /// </summary>
    public abstract class ContentOwnerMessage : ContentMessage
    {
        //once SenderUserId was removed, it can be made into a non-abstract member
        public abstract int OwnerId { get; set; }
    }
}
