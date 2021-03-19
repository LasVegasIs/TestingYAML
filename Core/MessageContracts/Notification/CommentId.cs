namespace Crey.MessageContracts.Notification
{
    public class CommentId
    {
        public string Scope { get; set; }
        public string TargetId { get; set; }
        public string Id { get; set; }

        //[Obsolete("only for backward compatibility, remove with " + Features.DisableLevelComment)]
        public string Target => $"{Scope}-{TargetId}";
    }
}
