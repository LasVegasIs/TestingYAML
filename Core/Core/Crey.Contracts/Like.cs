using Crey.Contracts.XportEnums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    [DataContract]
    public class LikeData
    {
        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public LikeTargetType TargetType { get; set; }

        [DataMember]
        public int TargetId { get; set; }
    }

    [DataContract]
    public class LikeParam
    {
        [DataMember]
        public LikeTargetType TargetType { get; set; }

        [DataMember]
        public int TargetId { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (TargetType == LikeTargetType.Unknown) throw new CreyException(ErrorCodes.InvalidArgument, "Unknown target type");
            if (TargetId <= 0) throw new CreyException(ErrorCodes.InvalidArgument, "Invalid target id");
        }
    }

    [DataContract]
    public class SingleLikeResult
    {
        [DataMember]
        public LikeData LikeData { get; set; }
    }

    [DataContract]
    public class LikeListResult
    {
        [DataMember]
        public List<LikeData> LikeList { get; set; }
    }
}
