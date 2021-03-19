using System;
using MessagingCore;

namespace UserProfileMessageContracts
{
    public interface IUserProfileMessage: IMessageContract
    {
        public const string TOPIC = "user-profile";
    }

    public abstract class UserProfileMessage : IUserProfileMessage
    {
        public abstract string Type { get; }
        public int AccountId { get; set; }
    }

    public class UserProfileDeactivated : UserProfileMessage
    {
        public override string Type => "UserProfileDeactivated";
    }

    public class UserProfileChanged : UserProfileMessage
    {
        public override string Type => "UserProfileChanged";

        public string DisplayName { get; set; }
        public Uri PublicImageUrl { get; set; }
    }
}
