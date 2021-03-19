using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.UserProfile
{
    [MessageSerde("UserXPLevelChanged")]
    public class UserXPLevelChangedMessage : UserProfileMessage
    {
        public uint Level { get; set; }
        public ulong LevelXP { get; set; }
        [Required]
        public string Name { get; set; }
        public uint LevelBeforeChange { get; set; }

        public ulong? PrevLevelXPValue { get; set; }
    }
}
