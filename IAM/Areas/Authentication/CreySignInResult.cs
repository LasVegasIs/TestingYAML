using IAM.Areas.Authentication.Models;
using Microsoft.AspNetCore.Identity;

namespace IAM.Areas.Authentication
{
    public class CreySignInResult
    {
        public SignInResult SignInResult { get; set; }
        public bool CanBeRestored { get; set; }
        public CanBeRestoredDetail CanBeRestoredDetail { get; set; }
    }
}
