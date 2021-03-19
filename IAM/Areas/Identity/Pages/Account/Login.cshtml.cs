using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using IAM.Data;
using IAM.Areas.Identity.Extensions;
using Microsoft.AspNetCore.Http;
using IAM.Areas.Authentication;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CreySignInManager _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(CreySignInManager signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }
        public string MobileMode { get; set; }
        public string RegistrationPageReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Email or username")]
            [Required]
            public string EmailOrUserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null, string theme = null, string mobile = null, string registrationPageReturnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl;
            MobileMode = mobile;
            RegistrationPageReturnUrl = registrationPageReturnUrl;

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            this.UpdateTheme(theme);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null, string mobile = null)
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
            MobileMode = mobile;

            if (!ModelState.IsValid)
            {
                // If we got this far, something failed, redisplay form
                return Page();
            }

            var user = await _userManager.FindByNameAsync(Input.EmailOrUserName) ??
                       await _userManager.FindByEmailAsync(Input.EmailOrUserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.CreyPasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.SignInResult.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToPage("./Redirect", new { redirectUrl = returnUrl, mobile });
            }
            if (result.SignInResult.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.SignInResult.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            if (result.CanBeRestored)
            {
                return RedirectToPage("./CancelPersonalDataDeletion");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
