using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAM.Data
{
    public class DBSoftDeletedUserAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccountId { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
    }
}
