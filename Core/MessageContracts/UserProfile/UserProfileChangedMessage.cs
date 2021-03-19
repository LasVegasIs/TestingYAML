using System;

namespace Crey.MessageContracts.UserProfile
{
    [MessageSerde("UserProfileChanged")]
    public class UserProfileChangedMessage : UserProfileMessage
    {
        public string DisplayName { get; set; }
        public Uri PublicImageUrl { get; set; }
    }
}
