using Crey.Contracts.XportEnums;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    [DataContract]
    public class ResourceRename
    {
        [DataMember]
        public string OldName { get; set; }

        [DataMember]
        public string NewName { get; set; }

        public override string ToString()
        {
            return $"on:[{OldName}] nn:[{NewName}]";
        }
    }

    [DataContract]
    public class ResourceInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public ResourceType Type { get; set; }

        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public int Owner { get; set; }

        [DataMember]
        public ResourceKind Kind { get; set; }

        [DataMember]
        public string StoragePath { get; set; }

        public string BlobPath()
        {
            return CreyNamingConvention.GetVersionedStoragePath(Version, StoragePath);
        }

        public override string ToString()
        {
            return $"i:{Id} v:[{Version}] t:{Type} k:{Kind} n:[{Name}] sp:[{StoragePath}] h:{Hash}";
        }
    }

    [DataContract]
    public class ResourceInfoInitial
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string Hash { get; set; }
    }

    [DataContract]
    public class ResourceDeployChange
    {
        [DataMember]
        public string Action { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string OldVersion { get; set; }

        [DataMember]
        public string OldStoragePath { get; set; }

        [DataMember]
        public string NewVersion { get; set; }

        [DataMember]
        public string NewStoragePath { get; set; }

        public string OldBlobPath()
        {
            return CreyNamingConvention.GetVersionedStoragePath(OldVersion, OldStoragePath);
        }

        public string NewBlobPath()
        {
            return CreyNamingConvention.GetVersionedStoragePath(NewVersion, NewStoragePath);
        }

        public override string ToString()
        {
            return $"i:{Id} ov:[{OldVersion}] osp:[{OldStoragePath}] nv:[{NewVersion}] nsp:[{NewStoragePath}] ";
        }
    }

    #region Badge
    [DataContract]
    public class BadgeData
    {
        [DataMember]
        public int LevelId { get; set; }

        [DataMember]
        public int AccountId { get; set; }

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
    #endregion
}