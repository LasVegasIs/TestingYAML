using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAM.Data
{
    public class ApplicationUser : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int AccountId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [PersonalData]
        public DateTimeOffset Creation { get; set; }

        [PersonalData]
        public bool NewsletterSubscribed { get; set; }
    }
}
