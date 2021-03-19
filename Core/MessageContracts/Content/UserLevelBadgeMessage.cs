namespace Crey.MessageContracts.Content
{
    [MessageSerde("UserLevelBadgeMessage")]
    public class UserLevelBadgeMessage : ContentUserMessage
    {
        public override int UserId { get; set; }
        public int LevelId { get; set; }
        public string BadgeName { get; set; }
        public string BadgeGuid { get; set; }
        public ulong CountOnAllLevels { get; set; }

        public UserLevelBadgeMessage()
        {
        }

        public UserLevelBadgeMessage(int userId, int levelId, string badgeName, string badgeGuid, ulong countOnAllLevels)
        {
            UserId = userId;
            LevelId = levelId;
            BadgeName = badgeName;
            BadgeGuid = badgeGuid;
            CountOnAllLevels = countOnAllLevels;
        }
    }
}
