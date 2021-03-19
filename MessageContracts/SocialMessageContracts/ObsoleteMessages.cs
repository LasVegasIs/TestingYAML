using System;

namespace SocialMessageContracts
{
    public class ThumbnailVoteMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public int LevelId { get; set; }
        public int CurrentCount { get; set; }

        public ThumbnailVoteMessage()
            : base("ThumbnailVote")
        {
        }
    }

    public class DeprecatedBadgeEarnedMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public string BadgeName { get; set; }
        public int LevelId { get; set; }
        public string BadgeGuid { get; set; }

        public DeprecatedBadgeEarnedMessage()
            : base("BadgeEarned")
        {
        }
    }

    public class FollowMessage : SocialMessage
    {
        public long Count { get; set; }
        public int FollowTarget { get; set; }
        public int FollowedByCount { get; set; }
        public int FollowingCount { get; set; }

        public FollowMessage()
            : base("Follow")
        {
        }
    }

    public class DeprecatedLevelLikeMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public int Count { get; set; }
        public int LevelId { get; set; }
        public ulong LevelLikeCount { get; set; }
        public ulong TotalLevelLikeCount { get; set; }
        public bool IsLike { get; set; }

        public DeprecatedLevelLikeMessage()
            : base("LevelLike")
        {
        }
    }

    [Obsolete]
    public class DeprecatedLevelCommentMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public ulong Count { get; set; }
        public string CommentId { get; set; }
        public int LevelId { get; set; }

        public DeprecatedLevelCommentMessage()
            : base("Comment")   // todo: rename to LevelComment
        {
        }
    }

    public class DeprecatedOwnerLevelCommentMessage : SocialMessage
    {
        public int LevelId { get; set; }
        public int OwnerId { get; set; }
        public ulong TotalCountOnLevel { get; set; }
        public ulong TotalCountsOnAllOwnedLevels { get; set; }

        public DeprecatedOwnerLevelCommentMessage(int levelId, int ownerId, ulong totalCountsOnAllOwnedLevels, ulong totalOnLevel)
            : base("OwnerLevelCommentMessage")
        {
            LevelId = levelId;
            OwnerId = ownerId;
            TotalCountOnLevel = totalOnLevel;
            TotalCountsOnAllOwnedLevels = totalCountsOnAllOwnedLevels;
        }
    }

    public class DeprecatedUserLevelCommentMessage : SocialMessage
    {
        public int LevelId { get; set; }
        public int UserId { get; set; }
        public ulong TotalCountOnAllLevels { get; set; }

        public DeprecatedUserLevelCommentMessage(int levelId, int userId, ulong totalCountOnAllLevels)
            : base("UserLevelCommentMessage")
        {
            LevelId = levelId;
            UserId = userId;
            TotalCountOnAllLevels = totalCountOnAllLevels;
        }
    }


    public class AchievementUnlockedMessage : SocialMessage
    {
        
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public string AchievementName { get; set; }
        public string AchievementNameDisplayName { get; set; }

        public AchievementUnlockedMessage()
            : base("AchievementUnlocked")
        {
        }
    }

    public class AchievementAlmostUnlockedMessage : SocialMessage
    {
        public int OwnerId { get; set; }
        public long Count { get; set; }
        public string AchievementName { get; set; }
        public string AchievementNameDisplayName { get; set; }

        public AchievementAlmostUnlockedMessage()
            : base("AchievementAlmostUnlocked")
        {

        }
    }
}
