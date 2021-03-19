using System.ComponentModel.DataAnnotations;

namespace UserProfileMessageContracts
{
    public class UserXPLevelChanged : UserProfileMessage
    {
        public override string Type => "UserXPLevelChanged";

        public uint Level { get; set; }
        public ulong LevelXP { get; set; }
        [Required]
        public string Name { get; set; }
        public uint LevelBeforeChange { get; set; }

        public ulong? PrevLevelXPValue { get; set; }
    }
}
