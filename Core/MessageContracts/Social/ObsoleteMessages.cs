namespace Crey.MessageContracts.Social
{
    [MessageSerde("ThumbnailVote")]
    public sealed class ThumbnailVoteMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int OwnerId { get; set; }
        public int LevelId { get; set; }
        public int CurrentCount { get; set; }

        public ThumbnailVoteMessage()
        {
        }
    }

    [MessageSerde("BadgeEarned")]
    public sealed class DeprecatedBadgeEarnedMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int OwnerId { get; set; }
        public string BadgeName { get; set; }
        public int LevelId { get; set; }
        public string BadgeGuid { get; set; }

        public DeprecatedBadgeEarnedMessage()
        {
        }
    }

    [MessageSerde("Follow")]
    public sealed class FollowMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public long Count { get; set; }
        public int FollowTarget { get; set; }
        public int FollowedByCount { get; set; }
        public int FollowingCount { get; set; }

        public FollowMessage()
        {
        }
    }

    [MessageSerde("OwnerLevelCommentMessage")]
    public sealed class DeprecatedOwnerLevelCommentMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
        public int OwnerId { get; set; }
        public ulong TotalCountOnLevel { get; set; }
        public ulong TotalCountsOnAllOwnedLevels { get; set; }

        public DeprecatedOwnerLevelCommentMessage() { }

        public DeprecatedOwnerLevelCommentMessage(int levelId, int ownerId, ulong totalCountsOnAllOwnedLevels, ulong totalOnLevel)
        {
            LevelId = levelId;
            OwnerId = ownerId;
            TotalCountOnLevel = totalOnLevel;
            TotalCountsOnAllOwnedLevels = totalCountsOnAllOwnedLevels;
        }
    }

    [MessageSerde("UserLevelCommentMessage")]
    public sealed class DeprecatedUserLevelCommentMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int LevelId { get; set; }
        public int UserId { get; set; }
        public ulong TotalCountOnAllLevels { get; set; }


        public DeprecatedUserLevelCommentMessage() { }
        public DeprecatedUserLevelCommentMessage(int levelId, int userId, ulong totalCountOnAllLevels)
        {
            LevelId = levelId;
            UserId = userId;
            TotalCountOnAllLevels = totalCountOnAllLevels;
        }
    }


    [MessageSerde("AchievementUnlocked")]
    public sealed class AchievementUnlockedMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public string AchievementName { get; set; }
        public string AchievementNameDisplayName { get; set; }

        public AchievementUnlockedMessage()
        {
        }
    }

    [MessageSerde("AchievementAlmostUnlocked")]
    public sealed class AchievementAlmostUnlockedMessage : SocialMessage
    {
        public int SenderUserId { get; set; }
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public string AchievementName { get; set; }
        public string AchievementNameDisplayName { get; set; }

        public AchievementAlmostUnlockedMessage()
        {
        }
    }
}
