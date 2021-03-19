using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Crey.Kernel.ServiceDiscovery;
using Crey.MessageStream.ServiceBus;
using IAM.Areas.Authentication;
using IAM.Areas.Authentication.Models;
using IAM.Areas.Authentication.ServiceBusMessages;
using IAM.Areas.Identity.Extensions;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalRegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICreyService<RegistrationHandler> _registrationHandler;
        private readonly ILogger<ExternalRegisterModel> _logger;
        private readonly CreyMessageBroker<IAccountServiceBusMessage> _accountMessageBroker;

        public ExternalRegisterModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ICreyService<RegistrationHandler> registrationHandler,
            ILogger<ExternalRegisterModel> logger,
            CreyMessageBroker<IAccountServiceBusMessage> accountMessageBroker)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _registrationHandler = registrationHandler;
            _logger = logger;
            _accountMessageBroker = accountMessageBroker;
        }

        [BindProperty]
        public ExternalUserInputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string Email { get; set; }

        public string ReturnUrl { get; set; }

        public string CreyTicket { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public string BirthDateErrorMessage { get; set; }        

        public async Task<IActionResult> OnGetAsync(int? avatarid, string username, string creyTicket = null, string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            LoginProvider = info.LoginProvider;
            Email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ReturnUrl = returnUrl;

            CreyTicket = creyTicket;

            Input = new ExternalUserInputModel { UserName = username, AvatarId = avatarid };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string provider, string creyTicket, string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            CreyTicket = creyTicket;

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            LoginProvider = info.LoginProvider;
            Email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                DateOfBirthValidationResult dateOfBirthValidationResult = DateOfBirth.IsValid(Input.Year.Value, Input.Month.Value, Input.Day.Value);
                if (!dateOfBirthValidationResult.IsValid)
                {
                    BirthDateErrorMessage = dateOfBirthValidationResult.ErrorMessage;
                    return Page();
                }

                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Email,
                    EmailConfirmed = false,
                    NewsletterSubscribed = Input.NewsletterSubscribed,
                };
                var utcDateOfBirth = new DateTime(Input.Year.Value, Input.Month.Value, Input.Day.Value);

                var result = await _registrationHandler.Value.RegisterUserAsync(user, null, utcDateOfBirth);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                        await _accountMessageBroker.SendMessage(new RegisteredServiceBusMessage { AccountId = user.AccountId, CreyTicket = CreyTicket });
                        await _registrationHandler.Value.AfterRegistrationAsync(user, utcDateOfBirth, Url, CreyTicket, Input.AvatarId);
                        return RedirectToPage("./Redirect", new { redirectUrl = returnUrl });
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code.TranslateModelErrorCode(), error.Description);
                }
            }

            return Page();
        }
    }
}
