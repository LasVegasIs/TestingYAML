using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IAM.Areas.Authentication.Models
{
    public class ExternalUserInputModel
    {
        [Required(ErrorMessage = "Required field")]
        [StringLength(32, ErrorMessage = "The {0} length must be {2}-{1} characters.", MinimumLength = 4)]
        [RegularExpression("[a-zA-Z0-9-_'.]{4,32}", ErrorMessage = "Whitespace and special characters are not allowed.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Required field")]
        [Display(Name = "Year")]
        public int? Year { get; set; }
        [Required(ErrorMessage = "Required field")]
        [Display(Name = "Month")]
        public int? Month { get; set; }
        [Required(ErrorMessage = "Required field")]
        [Display(Name = "Day")]
        public int? Day { get; set; }

        [Display(Name = "Newsletter")]
        public bool NewsletterSubscribed { get; set; }

        [Display(Name = "Terms and Conditions")]
        [Compare("AreTermsAccepted", ErrorMessage = "You must accept the terms and conditions")]
        public bool AreTermsAccepted { get; set; }

        [HiddenInput]
        public int? AvatarId { get; set; }
    }
}
