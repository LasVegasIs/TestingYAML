using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    [DataContract]
    public enum LevelType
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Play,
        [EnumMember] Template,
    }

    [Flags]
    [DataContract]
    public enum LevelTag
    {
        [EnumMember] NoFilter = 0,
        [EnumMember] StaffPicked = 1 << 2,
        [EnumMember] MostPlayed = 1 << 3,
        [EnumMember] CreateFeatured = 1 << 4,
        [EnumMember] PlayFeatured = 1 << 5,
        [EnumMember] ChallengeWinner = 1 << 6,
        [EnumMember] PlayMultiplayer = 1 << 7,
        [EnumMember] PlaySingle = 1 << 8,
        [EnumMember] AllowPublish = 1 << 9,
        [EnumMember] ParticipateInVote = 1 << 10,
        [EnumMember] Moderated = 1 << 11,
        [EnumMember] BetaVersion = 1 << 12,
        [EnumMember] ModeratedThumbnail = 1 << 13,
        [EnumMember] AllowMobilePublish = 1 << 14,
    }

    public class LevelTagMasks
    {
        // tags cleared during level copy
        public const LevelTag Flags = LevelTag.StaffPicked | LevelTag.MostPlayed | LevelTag.CreateFeatured | LevelTag.PlayFeatured | LevelTag.ChallengeWinner;

        // tags preserved from template during level creation
        public const LevelTag Roles = LevelTag.PlayMultiplayer | LevelTag.PlaySingle | LevelTag.AllowPublish | LevelTag.ParticipateInVote | LevelTag.BetaVersion | LevelTag.AllowMobilePublish;
    }

    [DataContract]
    public enum LeaderboardScoreType
    {
        [EnumMember] ByTimeDescending,
        [EnumMember] ByTimeAscending,
        [EnumMember] ByScoreDescending,
        [EnumMember] ByScoreAscending,
    }

    [DataContract]
    public enum BadgePersistencyType
    {
        [EnumMember] NotPersistent = 0,
        [EnumMember] Persistent = 1,
    }

    [DataContract]
    public enum ChannelType
    {
        [EnumMember] Manual,    // Devs manually assign levels to this channel (eg Staff-picked)
        [EnumMember] Automatic, // Channel is populated automatically by a script (eg Most Liked)
        [EnumMember] Virtual    // No levels correspond to this channel but can be queried - queries use different methods to generate the results (eg Most Played)
    }

    public class LevelValidationException : Exception
    {
        public readonly List<PrefabInfo> ConflictingPrefabs;

        public LevelValidationException(List<PrefabInfo> prefabs)
        {
            this.ConflictingPrefabs = prefabs;
        }
    }

    [DataContract]
    public class LevelValidationError
    {
        [DataMember]
        public List<PrefabInfo> ConflictingResources { get; set; }
    }


    public class LeaderboardScore
    {
        public int Rank { get; set; } = int.MaxValue;
        public int AccountId { get; set; }
        public int Score { get; set; }
        public int Time { get; set; }
        public DateTime CreationTime { get; set; }
        public bool HasReplay { get; set; }
    }

    public class LeaderboardSingleResult
    {
        [DataMember]
        public LeaderboardScoreType ScoreType { get; set; }

        [DataMember]
        public LeaderboardScore UserScore { get; set; }
    }

    [DataContract]
    public class LeaderboardListResult
    {
        [DataMember]
        public LeaderboardScoreType ScoreType { get; set; }

        [DataMember]
        public List<LeaderboardScore> TopScores { get; set; }

        [DataMember]
        public LeaderboardScore BestUserScore { get; set; }

        [DataMember]
        public LeaderboardScore CurrentUserScore { get; set; }

        public void DefaultUserScores(int accountId)
        {
            if (BestUserScore == null)
            {
                // some "invalid" value for easier parsing
                BestUserScore = new LeaderboardScore
                {
                    AccountId = accountId,
                };
            }

            if (CurrentUserScore == null)
            {
                // some "invalid" value for easier parsing
                CurrentUserScore = new LeaderboardScore
                {
                    AccountId = accountId,
                };
            }
        }

    }


    /// <summary>
    /// Information about the collectible badges (no user specific info)
    /// </summary>
    public class BadgeInfo
    {
        public string BadgeGuid { get; set; }

        public string Icon { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CountRequired { get; set; }

        public BadgePersistencyType Persistent { get; set; }
    }


    /// <summary>
    /// /// Information about the collected badges, user specific info
    /// </summary>
    public class BadgeProgress
    {
        [DataMember]
        public string BadgeGuid { get; set; }

        [DataMember]
        public string Icon { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int CountRequired { get; set; }

        [DataMember]
        public int Count { get; set; }
    }

    public enum SystemGroups
    {
        Moderators = 1
    }
}
