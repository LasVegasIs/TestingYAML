using System.Threading.Tasks;
using Crey.Web.Analytics;
using IAM.Clients;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly AnalyticsClient _analyticsClient;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager, 
            ILogger<LogoutModel> logger,
            AnalyticsClient analyticsClient)
        {
            _signInManager = signInManager;
            _logger = logger;
            _analyticsClient = analyticsClient;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null, string mobile = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            _analyticsClient.SendLogoutEvent();

            if (returnUrl != null)
            {
                return RedirectToPage("./Redirect", new { redirectUrl = returnUrl, mobile });
            }
            else
            {
                return RedirectToPage("./Login", new { mobile });
            }
        }
    }
}
