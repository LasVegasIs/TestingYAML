namespace IAM.Areas.Authentication.Models
{
    public class CanBeRestoredDetail
    {        
        public string UserId { get; set; }

        /// <summary>
        /// Accuont can be restored if the value is True
        /// </summary>
        public bool CanBeRestored { get; set; }

        /// <summary>
        /// If the account has a password or not
        /// </summary>
        public bool RequirePassword { get; set; }
    }
}
