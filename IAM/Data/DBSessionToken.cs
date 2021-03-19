using System;
using System.ComponentModel.DataAnnotations;

namespace IAM.Data
{
    public enum CredentialType
    {
        UserPassword,   // direct EP
        LoginPage,      // form auth login page
        Facebook,
        Google,
        MultiFactorAuthentication,
        SingleAccessKey,
        MultiAccessKey,
        RefreshKey,

        /// <summary>
        /// The server needs impersonate user to act on its behalf.
        /// </summary>
        Impersonation
    }

    public class DBSessionToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public CredentialType Credential { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string UserAgent { get; set; }

        public string Ip { get; set; }

        public string Country { get; set; }

        [Required]
        public DateTimeOffset Issued { get; set; }

        public int RefreshCount { get; set; }

        public DateTimeOffset? LastRefreshed { get; set; }

        public DateTimeOffset? Revoked { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; } // optimistic concurency handling

    }
}
