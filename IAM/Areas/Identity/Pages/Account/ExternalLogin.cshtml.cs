using System.Security.Claims;
using System.Threading.Tasks;
using Crey.Exceptions;
using IAM.Areas.Authentication;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly CreySignInManager _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            CreySignInManager signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public string LoginProvider { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, int? avatarId, string username, string creyTicket, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { avatarId, username, creyTicket, returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(int? avatarId, string username, string creyTicket, string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                LoginProvider = info.LoginProvider;
                ErrorMessage = $"Make sure you have access to your { info.LoginProvider } account or try to choose a different option.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.CreyExternalLoginSignInAsync(email, info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (result.SignInResult.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return RedirectToPage("./Redirect", new { redirectUrl = returnUrl });
            }
            if (result.SignInResult.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            if (result.CanBeRestored)
            {
                return RedirectToPage("./CancelPersonalDataDeletion");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToPage("./ExternalRegister", new { ReturnUrl = returnUrl, AvatarId = avatarId, UserName = username, CreyTicket = creyTicket });
            }

            return RedirectToPage("./ExternalLinkAccount", new { ReturnUrl = returnUrl });
        }
    }
}
