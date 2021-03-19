using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Crey.Contracts
{

    public class BuyParams
    {
        [Required]
        public int? Packid { get; set; }
    }

    [DataContract]
    public class PrefabProviderInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string IconResource { get; set; }        // will be removed soon
    }


    [DataContract]
    public class PrefabPackInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int ProviderId { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ThumbResource { get; set; }       // will be removed

        [DataMember]
        public OwningPolicy OwningPolicy { get; set; }

        [DataMember]
        public bool IsOwned { get; set; }
    }


    [DataContract]
    public class PrefabProviderRegisterParams
    {
        [DataMember(IsRequired = true)]
        public string Title { get; set; }

        [DataMember(IsRequired = true)]
        public string Description { get; set; }
    }

    [DataContract]
    public class PrefabProviderUpdateParams
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class PrefabProviderUpdateResult
    {
        [DataMember]
        public PrefabProviderInfo PrefabProviderInfo { get; set; }

        [DataMember]
        public ResourceInfo IconResourceInfo { get; set; }
    }

    [DataContract]
    public class PrefabProviderSingleResult
    {
        [DataMember]
        public PrefabProviderInfo PrefabProviderInfo { get; set; }
    }

    [DataContract]
    public class PrefabProviderListResult
    {
        [DataMember]
        public List<PrefabProviderInfo> PrefabProviderInfos { get; set; }
    }


    [DataContract]
    public class PrefabPackRegisterParams
    {
        [DataMember(IsRequired = true)]
        public int ProviderId { get; set; }

        [DataMember(IsRequired = true)]
        public string Title { get; set; }

        [DataMember(IsRequired = true)]
        public string Description { get; set; }

        [DataMember(IsRequired = true)]
        public OwningPolicy OwningPolicy { get; set; }
    }

    [DataContract]
    public class PrefabPackUpdateParams
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }

        [DataMember]
        public int ProviderId { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public OwningPolicy OwningPolicy { get; set; }
    }

    [DataContract]
    public class PrefabPackListByProviderParams
    {
        [DataMember(IsRequired = true)]
        public int ProviderId { get; set; }
    }


    [DataContract]
    public class PrefabPackUpdateResult
    {
        [DataMember]
        public PrefabPackInfo PrefabPackInfo { get; set; }

        [DataMember]
        public ResourceInfo ThumbResourceInfo { get; set; }
    }

    [DataContract]
    public class PrefabPackSingleResult
    {
        [DataMember]
        public PrefabPackInfo PrefabPackInfo { get; set; }
    }

    [DataContract]
    public class PrefabPackListResult
    {
        [DataMember]
        public List<PrefabPackInfo> PrefabPackInfos { get; set; }
    }
}
