using Crey.Kernel.ServiceDiscovery;
using IAM.Areas.Authentication.Models;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication.Controller
{
    /// <summary>
    /// The controller is responsible for password actions
    /// </summary>
    [EnableCors]
    [AllowAnonymous]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICreyService<EmailSender> _emailSender;

        public PasswordController(
            UserManager<ApplicationUser> userManager,
            ICreyService<EmailSender> emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Generate password reset token and send ForgotPassword email.
        /// </summary>
        /// <param name="inputModel">Forgot password input model.</param>
        /// <returns>The action result.</returns>
        [HttpPost("/iam/api/v1/accounts/forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordInputModel inputModel)
        {
            var user = await _userManager.FindByEmailAsync(inputModel.Email);
            if (user == null)
            {
                return NotFound($"User with email {inputModel.Email} does not exist.");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            dynamic callbackUrlParams = new ExpandoObject();
            callbackUrlParams.area = "Identity";
            callbackUrlParams.code = code;

            if (!string.IsNullOrWhiteSpace(inputModel.MobileDeepLink) && 
                !inputModel.MobileDeepLink.StartsWith("http"))
            {
                callbackUrlParams.mobileDeepLink = inputModel.MobileDeepLink;
            }

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: (object)callbackUrlParams,
                protocol: "https");

            await _emailSender.Value.SendPasswordResetEmailAsync(inputModel.Email, callbackUrl);

            return Ok();
        }

        /// <summary>
        /// Reset password.
        /// </summary>
        /// <param name="inputModel">Reset password input model.</param>
        /// <returns>The action result.</returns>
        [HttpPost("/iam/api/v1/accounts/resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordInputModel inputModel)
        {
            var user = await _userManager.FindByEmailAsync(inputModel.Email);
            if (user == null)
            {
                return NotFound($"User with email {inputModel.Email} does not exist.");
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(inputModel.Code));

            var result = await _userManager.ResetPasswordAsync(user, code, inputModel.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
    }
}
