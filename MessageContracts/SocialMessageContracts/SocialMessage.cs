using System;
using MessagingCore;

namespace SocialMessageContracts
{
    public interface ISocialMessage: IMessageContract
    {
        public const string TOPIC = "social";
    }

    [Obsolete("use ISocialMessage")]
    public abstract class SocialMessage : ISocialMessage
    {
        public string Type { get; private set; }
        public int SenderUserId { get; set; }

        public SocialMessage(string type)
        {
            Type = type;
        }
    }
}
