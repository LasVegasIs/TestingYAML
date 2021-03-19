using System;

namespace Crey.MessageContracts.Social
{
    [Obsolete("use UserProfileChanged instead")]
    [MessageSerde("ProfilePicture")]
    public sealed class ProfilePictureSetMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public ProfilePictureSetMessage()
        {
        }
    }
}
