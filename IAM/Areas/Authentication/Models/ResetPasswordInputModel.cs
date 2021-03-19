using System.ComponentModel.DataAnnotations;

namespace IAM.Areas.Authentication.Models
{
    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "Required field")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required field")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
