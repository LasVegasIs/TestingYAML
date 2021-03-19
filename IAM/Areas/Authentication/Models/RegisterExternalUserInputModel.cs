namespace IAM.Areas.Authentication.Models
{
    public class RegisterExternalUserInputModel
    {
        public ExternalUserInputModel User { get; set; }

        public string CreyTicket { get; set; }
    }
}
