namespace IAM.Areas.Authentication.Models
{
    public class ExternalLoginModel
    {
        public string LoginProvider { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
