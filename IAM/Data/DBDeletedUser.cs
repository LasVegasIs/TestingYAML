using System;
using System.ComponentModel.DataAnnotations;

namespace IAM.Data
{
    public class DBDeletedUser
    {
        [Key]
        public int AccountId { get; set; }
        public DateTime SoftDeletionTime { get; set; }
        public DateTime HardDeletionTime { get; set; }
    }
}
