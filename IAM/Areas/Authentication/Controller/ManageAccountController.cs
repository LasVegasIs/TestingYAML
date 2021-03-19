using Crey.Exceptions;
using Crey.MessageStream.ServiceBus;
using IAM.Areas.Authentication.Models;
using IAM.Areas.Authentication.ServiceBusMessages;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication.Controller
{
    /// <summary>
    /// The controller is responsible for changing your account settings.
    /// </summary>
    [EnableCors]
    [ApiController]
    public class ManageAccountController : ControllerBase
    {
        private readonly CreyMessageBroker<IAccountServiceBusMessage> _accountMessageBroker;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ManageAccountController(
            CreyMessageBroker<IAccountServiceBusMessage> accountMessageBroker,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _accountMessageBroker = accountMessageBroker;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Restore deleted account
        /// </summary>
        /// <param name="inputModel">Restore account input model</param>
        /// <returns>Action result</returns>
        [AllowAnonymous]
        [HttpPost("/iam/api/v1/accounts/manage/restore")]
        public async Task<IActionResult> RestoreAccount([FromBody] RestoreAccountInputModel inputModel)
        {
            var user = await _userManager.FindByIdAsync(inputModel.UserId) ??
                throw new AccountNotFoundException($"User with id {inputModel.UserId} was not found.");            

            var requirePassword = await _userManager.HasPasswordAsync(user);
            if (requirePassword)
            {
                if (string.IsNullOrWhiteSpace(inputModel.Password))
                {
                    throw new PasswordException("User or password is not valid.");
                }

                if (!await _userManager.CheckPasswordAsync(user, inputModel.Password))
                {
                    throw new PasswordException("User or password is not valid.");
                }
            }

            await _accountMessageBroker.SendMessage(new CancelSoftDeleteServiceBusMessage { AccountId = user.AccountId });

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }

            return Ok();
        }
    }
}
