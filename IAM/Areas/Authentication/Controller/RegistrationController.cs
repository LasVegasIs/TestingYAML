using Crey.Kernel.ServiceDiscovery;
using Crey.MessageStream.ServiceBus;
using IAM.Areas.Authentication.Models;
using IAM.Areas.Authentication.ServiceBusMessages;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication.Controller
{
    /// <summary>
    /// The controller is responsible for registering new users.
    /// </summary>
    [EnableCors]
    [AllowAnonymous]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterUserInputModel> _logger;
        private readonly ICreyService<RegistrationHandler> _registrationHandler;
        private readonly CreyMessageBroker<IAccountServiceBusMessage> _accountMessageBroker;

        public RegistrationController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RegisterUserInputModel> logger,
            ICreyService<RegistrationHandler> registrationHandler,
            CreyMessageBroker<IAccountServiceBusMessage> accountMessageBroker)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _registrationHandler = registrationHandler;
            _accountMessageBroker = accountMessageBroker;
        }

        /// <summary>
        /// Register user with email and password.
        /// </summary>
        /// <param name="inputModel">Register user input model.</param>
        /// <returns>Action result.</returns>
        [HttpPost("/iam/api/v1/accounts/register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserInputModel inputModel)
        {
            _registrationHandler.Value.BeforeRegistration(inputModel.CreyTicket);
            var inputUser = inputModel.User;

            DateOfBirthValidationResult dateOfBirthValidationResult = DateOfBirth.IsValid(
                inputUser.Year.Value,
                inputUser.Month.Value,
                inputUser.Day.Value);

            if (!dateOfBirthValidationResult.IsValid)
            {
                return BadRequest(dateOfBirthValidationResult.ErrorMessage);
            }

            var user = new ApplicationUser
            {
                UserName = inputUser.UserName,
                Email = inputUser.Email,
                EmailConfirmed = false,
                NewsletterSubscribed = inputUser.NewsletterSubscribed,
            };

            var utcDateOfBirth = new DateTime(inputUser.Year.Value, inputUser.Month.Value, inputUser.Day.Value);

            var result = await _registrationHandler.Value.RegisterUserAsync(user, inputUser.Password, utcDateOfBirth);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User created a new account with password.");
            await _signInManager.SignInAsync(user, isPersistent: true, CredentialType.LoginPage.ToString());
            await _accountMessageBroker.SendMessage(new RegisteredServiceBusMessage { AccountId = user.AccountId, CreyTicket = inputModel.CreyTicket });
            await _registrationHandler.Value.AfterRegistrationAsync(user, utcDateOfBirth, Url, inputModel.CreyTicket, inputUser.AvatarId);

            return Ok();
        }

        /// <summary>
        /// Register user with external provider.
        /// </summary>
        /// <param name="inputModel">Register external user input model</param>
        /// <returns>Action result</returns>
        [HttpPost("/iam/api/v1/accounts/externalregister")]
        public async Task<IActionResult> ExternalRegisterUser([FromBody] RegisterExternalUserInputModel inputModel)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Unauthorized("Error loading external login information.");
            }

            var inputUser = inputModel.User;

            DateOfBirthValidationResult dateOfBirthValidationResult = DateOfBirth.IsValid(
                inputUser.Year.Value,
                inputUser.Month.Value,
                inputUser.Day.Value);

            if (!dateOfBirthValidationResult.IsValid)
            {
                return BadRequest(dateOfBirthValidationResult.ErrorMessage);
            }

            var user = new ApplicationUser
            {
                UserName = inputUser.UserName,
                Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                EmailConfirmed = false,
                NewsletterSubscribed = inputUser.NewsletterSubscribed
            };

            var utcDateOfBirth = new DateTime(inputUser.Year.Value, inputUser.Month.Value, inputUser.Day.Value);

            var result = await _registrationHandler.Value.RegisterUserAsync(user, null, utcDateOfBirth);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(x => x.Code, x => x.Description);
                return NotFound(errors);
            }

            result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(x => x.Code, x => x.Description);
                return NotFound(errors);
            }

            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
            await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
            await _accountMessageBroker.SendMessage(new RegisteredServiceBusMessage { AccountId = user.AccountId, CreyTicket = inputModel.CreyTicket });
            await _registrationHandler.Value.AfterRegistrationAsync(user, utcDateOfBirth, Url, inputModel.CreyTicket, inputUser.AvatarId);

            return Ok();
        }
    }
}
