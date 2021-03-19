using System.ComponentModel.DataAnnotations;

namespace IAM.Areas.Authentication.Models
{
    public class RestoreAccountInputModel
    {
        [Required(ErrorMessage = "Required field")]
        public string UserId { get; set; }

        public string Password { get; set; }
    }
}
