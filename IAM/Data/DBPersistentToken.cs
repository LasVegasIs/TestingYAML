using System;
using System.ComponentModel.DataAnnotations;

namespace IAM.Data
{
    public class DBPersistentToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTimeOffset Issued { get; set; }

        public DateTimeOffset? Revoked { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; } // optimistic concurency handling

    }
}
