using System.ComponentModel.DataAnnotations;

namespace IAM.Areas.Authentication.Models
{
    public class TwoFactorAuthenticationModel
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        [Required(ErrorMessage = "Required field")]
        public string UserId { get; set; }

        /// <summary>
        /// Flag indicating whether the current browser should be remember, suppressing all further two factor authentication prompts
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// The two factor authentication code to validate.
        /// </summary>
        [Required(ErrorMessage = "Required field")]
        public string Code { get; set; }

        /// <summary>
        /// Flag indicating whether the sign-in cookie should persist after the browser is closed. Default value true
        /// </summary>
        public bool IsPersistent { get; set; }
    }
}
