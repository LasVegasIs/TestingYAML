using System;
using System.Runtime.Serialization;

namespace Crey.Contracts.XportEnums
{
    ///             Important: Add new stuff strictly to the end !
    [DataContract]
    public enum ResourceType
    {
        [EnumMember] Unknown,
        [EnumMember] Initial,
        [EnumMember] User,
        [EnumMember] Content,
    }


    [DataContract]
    public enum ResourceKind
    {
        [EnumMember] Unknown,
        [EnumMember] LevelJson,
        [EnumMember] LevelThmbnail,
        [EnumMember] PrefabJson,
        [EnumMember] PrefabIcon,
        [EnumMember] PrefabProviderIcon,
        [EnumMember] PrefabPackThumbnail,
    }

    [DataContract]
    public enum LevelPublishState
    {
        [EnumMember] Unknown,
        [EnumMember] Public,
        [EnumMember] Private,
        [EnumMember] Deleted
    }

    [DataContract]
    public enum CommentTag
    {
        [EnumMember] Flagged = 1 << 0,  // marked for further investigation
        [EnumMember] Banned = 1 << 1,  // removed by moderators
        [EnumMember] Deleted = 1 << 2,  // deleted by user
        [EnumMember] Edited = 1 << 3,  // edited by user
    }

    [DataContract]
    public enum LikeTargetType
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Level,
        [EnumMember] Comment,
    }


    [DataContract]
    public enum SourceFormat
    {
        [EnumMember] Unknown,
        [EnumMember] Cpp,
        [EnumMember] Rust,
    }
}