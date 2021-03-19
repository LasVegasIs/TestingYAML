namespace Crey.MessageContracts.Content
{
    [MessageSerde("OwnerLevelBadgeMessage")]
    public sealed class OwnerLevelBadgeMessage : ContentOwnerMessage
    {
        public override int OwnerId { get; set; }
        public int LevelId { get; set; }
        public string BadgeName { get; set; }
        public string BadgeGuid { get; set; }
        public uint CountOnLevel { get; set; }
        public ulong CountOnAllLevels { get; set; }

        public OwnerLevelBadgeMessage() { }
        public OwnerLevelBadgeMessage(int ownerId, int levelId, string badgeName, string badgeGuid, uint countOnLevel, ulong countOnAllLevels)
        {
            OwnerId = ownerId;
            LevelId = levelId;
            BadgeName = badgeName;
            BadgeGuid = badgeGuid;
            CountOnLevel = countOnLevel;
            CountOnAllLevels = countOnAllLevels;
        }
    }
}
