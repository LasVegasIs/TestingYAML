using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Crey.Contracts.XportContracts;
using Crey.Contracts.XportEnums;

namespace Crey.Contracts
{
    #region Params
    [DataContract]
    public class PlayCountParam
    {
        [DataMember]
        [Required]
        [Range(1, int.MaxValue)]
        public int LevelId { get; set; }
    }
    #endregion

    #region Results
    [DataContract]
    public class PlayCountResult
    {
        [DataMember]
        public int LevelId { get; set; }

        [DataMember]
        public int PlayCount { get; set; }
    }
    #endregion
}
