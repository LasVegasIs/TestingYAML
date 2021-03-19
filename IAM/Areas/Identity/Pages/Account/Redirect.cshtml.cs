using IAM.Areas.Identity.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RedirectModel : PageModel
    {
        public string RedirectUrl;
        public string MobileMode;

        public RedirectModel()
        {
        }

        public void OnGet(string redirectUrl, string mobile = null)
        {
            RedirectUrl = redirectUrl;
            MobileMode = mobile;
        }

        public IActionResult OnPost(string redirectUrl, string mobile = null)
        {
            return this.WhitelistedRedirect(redirectUrl, mobile);
        }
    }
}
