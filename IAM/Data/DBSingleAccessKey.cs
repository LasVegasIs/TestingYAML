using System;
using System.ComponentModel.DataAnnotations;

namespace IAM.Data
{
    public class DBSingleAccessKey
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public DateTimeOffset Issued { get; set; }

        public DateTimeOffset? Used { get; set; }

        [Required]
        public string Key { get; set; }
    }
}
