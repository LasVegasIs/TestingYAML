using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Kernel.IAM;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web;
using IAM.Areas.Authentication;
using IAM.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IAM.Areas.Identity.Extensions;
using Crey.MessageStream.ServiceBus;
using IAM.Areas.Authentication.ServiceBusMessages;
using IAM.Areas.Authentication.Models;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ReCaptcha _reCaptcha;
        private readonly ICreyService<RegistrationHandler> _registrationHandler;
        private readonly CreyMessageBroker<IAccountServiceBusMessage> _accountMessageBroker;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IConfiguration configuration,
            ReCaptcha reCaptcha,
            ICreyService<RegistrationHandler> registrationHandler,
            CreyMessageBroker<IAccountServiceBusMessage> accountMessageBroker)
        {
            _signInManager = signInManager;
            _logger = logger;
            _reCaptcha = reCaptcha;
            _registrationHandler = registrationHandler;
            _accountMessageBroker = accountMessageBroker;
            ReCaptchaClientKey = configuration.GetReCaptchaSiteKey(IAMDefaults.SERVICE_NAME);
        }

        [BindProperty]
        public UserInputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public string RegistrationPageReturnUrl { get; set; }
        public string MobileMode { get; set; }

        public string ReCaptchaClientKey { get; set; }

        public string BirthDateErrorMessage { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string CreyTicket { get; set; }

        public string CreyTheme { get; set; }

        public async Task OnGetAsync([FromQuery] string username = null, string returnUrl = null, string registrationPageReturnUrl = null, string creyTicket = null, string theme = null, string mobile = null)
        {
            ReturnUrl = returnUrl;
            RegistrationPageReturnUrl = registrationPageReturnUrl;
            MobileMode = mobile;
            Input = new UserInputModel { UserName = username };
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            CreyTicket = creyTicket;
            this.UpdateTheme(theme);
        }

        public async Task<IActionResult> OnPostAsync(string creyTicket, string returnUrl = null, string registrationPageReturnUrl = null, string mobile = null)
        {
            ReturnUrl = returnUrl;
            RegistrationPageReturnUrl = registrationPageReturnUrl;
            MobileMode = mobile;
            CreyTicket = creyTicket;

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            _registrationHandler.Value.BeforeRegistration(CreyTicket);

            if (ModelState.IsValid)
            {
                DateOfBirthValidationResult dateOfBirthValidationResult = DateOfBirth.IsValid(Input.Year.Value, Input.Month.Value, Input.Day.Value);
                if (!dateOfBirthValidationResult.IsValid)
                {
                    BirthDateErrorMessage = dateOfBirthValidationResult.ErrorMessage;
                    return Page();
                }

                bool isReCaptchaValid = await _reCaptcha.IsResponseValid(Request.Form["g-recaptcha-response"]);
                if (!isReCaptchaValid)
                {
                    ModelState.AddModelError(string.Empty, "Captcha validation unsuccessful.");
                    return Page();
                }

                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    EmailConfirmed = false,
                    NewsletterSubscribed = Input.NewsletterSubscribed,
                };
                var utcDateOfBirth = new DateTime(Input.Year.Value, Input.Month.Value, Input.Day.Value);

                var result = await _registrationHandler.Value.RegisterUserAsync(user, Input.Password, utcDateOfBirth);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _signInManager.SignInAsync(user, isPersistent: true, CredentialType.LoginPage.ToString());
                    await _accountMessageBroker.SendMessage(new RegisteredServiceBusMessage { AccountId = user.AccountId, CreyTicket = CreyTicket });
                    await _registrationHandler.Value.AfterRegistrationAsync(user, utcDateOfBirth, Url, CreyTicket, null);

                    if (MobileMode == "true" && !string.IsNullOrEmpty(RegistrationPageReturnUrl))
                    {
                        return RedirectToPage("./Redirect", new { redirectUrl = RegistrationPageReturnUrl });
                    }

                    return RedirectToPage("./Redirect", new { redirectUrl = ReturnUrl });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code.TranslateModelErrorCode(), error.Description);
                }
            }
            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
