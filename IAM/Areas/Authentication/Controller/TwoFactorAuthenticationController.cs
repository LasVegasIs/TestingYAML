using Crey.Exceptions;
using IAM.Areas.Authentication.Models;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    [EnableCors]
    [ApiController]
    [Authorize]
    public class TwoFactorAuthenticationController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public TwoFactorAuthenticationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, 
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Twoes the factor authentication user. 
        /// For this method need cookies which should be containe Identity.TwoFactorUserId.
        /// In order to get Identity.TwoFactorUserId cookies, you need to perform any of the regular login methods 
        /// which will return this value (SignInWithEmailAsync method for examle.).
        /// </summary>
        /// <param name="twoFactorAuthentication">The two factor authentication model.</param>
        /// <returns>Result of two factor authentification user</returns>
        /// <response code="200">Returns when sign in is succeeded </response>
        /// <response code="400">If has some problem</response>   
        /// <response code="404">Data is not found</response>   
        [HttpPost("/iam/api/v1/twofactorauthentication/signin")]
        public async Task<IActionResult> TwoFactorAuthenticationUser([FromBody]TwoFactorAuthenticationModel twoFactorAuthentication)
        {
            if (!ModelState.IsValid) 
            {
                throw new HttpStatusErrorException(HttpStatusCode.BadRequest, Constants.ModelIsNotValidMessage);
            }

            var user = await _userManager.FindByIdAsync(twoFactorAuthentication.UserId);

            if (user == null) 
            {
                throw new HttpStatusErrorException(HttpStatusCode.NotFound, $"Unable to load user with ID '{twoFactorAuthentication.UserId}'.");
            }

            if (!user.TwoFactorEnabled) 
            {
                throw new HttpStatusErrorException(HttpStatusCode.BadRequest, "The current user is not using two-factor authentication.");
            }

            // Strip spaces and hypens
            var code = twoFactorAuthentication.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
  
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, twoFactorAuthentication.IsPersistent, twoFactorAuthentication.RememberMe);

            if (result.Succeeded) 
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);

                return Ok($"User with ID '{user.Id}' logged in with 2fa.");
            }

            if (result.IsLockedOut) 
            {
                _logger.LogWarning(Constants.UserIsLockedOutMessage, user.Id);

                throw new HttpStatusErrorException(HttpStatusCode.BadRequest, Constants.UserIsLockedOutMessage);
            }

            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);

            throw new HttpStatusErrorException(HttpStatusCode.BadRequest, $"Invalid authenticator code entered for user with ID '{user.Id}");
        }

        /// <summary>
        /// Gets the shared key for user. 
        /// Which is subsequently used to receive codes through the Microsoft mobile application.
        /// </summary>
        /// <returns>The shared key for user</returns>
        /// <response code="200">Returns shared key </response>
        /// <response code="404">Data is not found</response>   
        [HttpGet("/iam/api/v1/twofactorauthentication/sharedkey")]
        public async Task<ActionResult> GetSharedKey()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) 
            {
                throw new HttpStatusErrorException(HttpStatusCode.NotFound, $"Unable to load user with ID '{user.Id}'.");
            }

            var sharedKey = await Helpers.GetSharedKeyAsync(_userManager, user);

            return Ok(sharedKey);
        }

        /// <summary>
        /// Verify two factor token and enables the authenticator.
        /// </summary>
        /// <param name="twoFactorAuthentication">The two factor authentication model.</param>
        /// <returns>The two factor recovery codes</returns>
        /// <response code="200">Seted two factor enabled for user </response>
        /// <response code="404">Data is not found</response>   
        /// <response code="400">If has some problem</response>   
        [HttpPost("/iam/api/v1/twofactorauthentication/enableauthenticator")]
        public async Task<ActionResult> EnableAuthenticator([FromBody]TwoFactorAuthenticationModel twoFactorAuthentication)
        {
            var user = await _userManager.FindByIdAsync(twoFactorAuthentication.UserId);

            if (user == null) 
            {
                throw new HttpStatusErrorException(HttpStatusCode.NotFound, $"Unable to load user with ID '{twoFactorAuthentication.UserId}'.");
            }

            // Strip spaces and hypens
            var code = twoFactorAuthentication.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (!is2faTokenValid) 
            {
                throw new HttpStatusErrorException(HttpStatusCode.BadRequest, "Verification code is invalid.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", twoFactorAuthentication.UserId);

            if (await _userManager.CountRecoveryCodesAsync(user) == 0) 
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

                return Ok(recoveryCodes);
            }

            return Ok($"User with ID '{twoFactorAuthentication.UserId}' has enabled 2FA with an authenticator app and he already has recovery codes");
        }
    }
}