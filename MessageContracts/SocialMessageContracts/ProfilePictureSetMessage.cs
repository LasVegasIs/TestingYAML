using System;

namespace SocialMessageContracts
{
    [Obsolete("use UserProfileChanged instead")]
    public class ProfilePictureSetMessage : SocialMessage
    {
        public ProfilePictureSetMessage() : base("ProfilePicture")
        {
        }
    }
}
