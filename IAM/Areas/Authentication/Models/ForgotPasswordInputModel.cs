using System.ComponentModel.DataAnnotations;

namespace IAM.Areas.Authentication.Models
{
    public class ForgotPasswordInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Mobile deep link looks like "crey://levelid=247105&mode=multiplay&multiNew=client" 
        /// </summary>
        public string MobileDeepLink { get; set; }
    }
}
