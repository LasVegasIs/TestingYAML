using Crey.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace IAM.Data
{
    public partial class AuthToken
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        public string SiteId { get; set; }

        [Required]
        [StringLength(128)]
        public string Token { get; set; }

        [Required]
        public AccountRoles RoleMask { get; set; }  //TODO: remove this, not used anymore

        [Required]
        public string DownVersions { get; set; }    //TODO: remove this, not used anymore

        [Required]
        public string UpVersion { get; set; }       //TODO: remove this, not used anymore

        public DateTime LastLogin { get; set; }
        public int LoginCount { get; set; }
    }
}

