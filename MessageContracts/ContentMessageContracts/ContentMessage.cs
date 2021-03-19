using System;
using MessagingCore;

namespace ContentMessageContracts
{
    public interface IContentMessage: IMessageContract
    {
        public const string TOPIC = "content";
    }

    [Obsolete("Use IContentMessage or ContentUserMessage or ContentOwnerMessage")]
    public abstract class ContentMessage : IContentMessage
    {
        public string Type { get; private set; }
        public int SenderUserId { get; set; }
        public ContentMessage(string type)
        {
            Type = type;
        }
    }
}
