using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Crey.Contracts
{

    [DataContract]
    public enum PrefabKind
    {
        [EnumMember] Unknown,
        [EnumMember] InitialPrefab = 1,
        [EnumMember] InitialBox,
        [EnumMember] UserBox = 100,
    }

    [DataContract]
    public enum PrefabPublishState
    {
        [EnumMember] Unknown,
        [EnumMember] Public,
        [EnumMember] Private,
        [EnumMember] Deprecated,    // Not visible in gallery any more without dev role
        [EnumMember] Hidden,        // Not visible by users (ex characters)
    }

    [DataContract]
    public enum PrefabTargetType
    {
        [EnumMember] LevelContent,
        [EnumMember] Box,
    }

    public class PrefabInfo
    {
        public int Id { get; set; }

        public int Owner { get; set; }

        public PrefabKind Kind { get; set; }

        public OwningPolicy OwningPolicy { get; set; }

        public PrefabPublishState PublishState { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Group { get; set; }

        public string PrefabResource { get; set; }      // to be removed

        public string ThumbnailResource { get; set; }   // to be removed

        public string ThumbnailDisplay { get; set; }

        public bool IsOwned { get; set; }

        public bool IsMultiCompatible { get; set; }
        public string Tags { get; set; }

        public string Usages { get; set; }
    }

    [DataContract]
    public class GalleryRegisterParams
    {
        [DataMember(IsRequired = true)]
        public PrefabKind Kind { get; set; }

        [DataMember(IsRequired = true)]
        public OwningPolicy OwningPolicy { get; set; }

        [DataMember(IsRequired = true)]
        public PrefabPublishState PublishState { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember(IsRequired = false)]
        public string Description { get; set; } = "";

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public string ThumbnailDisplay { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (Kind == PrefabKind.Unknown) throw new CreyException(ErrorCodes.InvalidArgument);
            if (PublishState == PrefabPublishState.Unknown) throw new CreyException(ErrorCodes.InvalidArgument);
            if (Title == null && Title.Trim() == "") throw new CreyException(ErrorCodes.InvalidArgument);
            if (Group == null && Group.Trim() == "") throw new CreyException(ErrorCodes.InvalidArgument);
            if (ThumbnailDisplay == null && ThumbnailDisplay.Trim() == "") throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class GalleryUpdateParams
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }

        [DataMember(IsRequired = false)]
        public string Title { get; set; }

        [DataMember(IsRequired = false)]
        public OwningPolicy OwningPolicy { get; set; }

        [DataMember(IsRequired = false)]
        public PrefabPublishState PublishState { get; set; }

        [DataMember(IsRequired = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false)]
        public string Group { get; set; }

        [DataMember]
        public string ThumbnailDisplay { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (Id <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (PublishState == PrefabPublishState.Unknown) throw new CreyException(ErrorCodes.InvalidArgument);
            if (Title != null && Title.Trim() == "") throw new CreyException(ErrorCodes.InvalidArgument);
            if (Group != null && Group.Trim() == "") throw new CreyException(ErrorCodes.InvalidArgument);
            if (ThumbnailDisplay != null && ThumbnailDisplay.Trim() == "") throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class GalleryAssignToPackParams
    {
        [DataMember(IsRequired = true)]
        public int PackId { get; set; }

        [DataMember(IsRequired = true)]
        public int PrefabId { get; set; }
    }


    [DataContract]
    public class GallerySetUsagesParams
    {
        [DataMember(IsRequired = true)]
        public int PrefabId { get; set; }

        [DataMember(IsRequired = true)]
        public List<string> Usages { get; set; }
    }

    [DataContract]
    public class GalleryGetUsagesResult
    {
        [DataMember(IsRequired = true)]
        public List<string> Usages { get; set; }
    }

    [DataContract]
    public class GalleryListByPackTitleParams
    {
        [DataMember(IsRequired = true)]
        public string Provider { get; set; }

        [DataMember(IsRequired = true)]
        public string Pack { get; set; }
    }

    [DataContract]
    public class GalleryListAllParams
    {
        [DataMember(IsRequired = true)]
        public List<PrefabKind> Kinds { get; set; }
    }

    [DataContract]
    public class GalleryListByUsageParams
    {
        [DataMember(IsRequired = true)]
        public string Usage { get; set; }
    }

    [DataContract]
    public class GalleryValidationError
    {
        [DataMember]
        public List<int> ConflictingLevelContentIds { get; set; }
    }

    [DataContract]
    public class GalleryOperationResult
    {
        [DataMember]
        public PrefabInfo GalleryInfo { get; set; }

        [DataMember]
        public ResourceInfo PrefabResource { get; set; }

        [DataMember]
        public ResourceInfo ThumbnailResource { get; set; }
    }

    public class GalleryInfoSingleResult
    {
        [DataMember]
        public PrefabInfo GalleryInfo { get; set; }
    }
}
