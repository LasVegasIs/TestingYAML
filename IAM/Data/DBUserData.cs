using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IAM.Data
{
    public class DBUserData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccountId { get; set;}

        [PersonalData]
        public DateTime? DateOfBirth { get; set; }

        [PersonalData]
        public string Gender { get; set; }
    }
}
