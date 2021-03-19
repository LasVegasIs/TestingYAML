using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web.Service2Service;
using IAM.Areas.Authentication.Models;
using IAM.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    [EnableCors]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ICreyService<AccountRepository> _accountRepo;
        private readonly CreySignInManager _signInManager;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            ICreyService<AccountRepository> accountRepo,
            CreySignInManager signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _accountRepo = accountRepo;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public class DefautDisplayName
        {
            public string Name { get; set; }
        }

        [HttpGet("/iam/s2s/v1/accounts/{accountId}/defaultdisplayname")]
        [ServerToServer]
        public async Task<DefautDisplayName> GetDefaultDisplayName(int accountId)
        {
            if (accountId <= 0)
                throw new Crey.Exceptions.InvalidArgumentException($"Invalid account id {accountId}");
            return new DefautDisplayName
            {
                Name = await _accountRepo.Value.GetDefaultDisplayName(accountId),
            };
        }

        public class SignInWithEmailParams
        {
            [Required]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "EmailOrUsername length should be between 3 and 100.")]
            public string EmailOrUserName { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Password length should be between 3 and 100.")]
            public string Password { get; set; }
        }

        /// <summary>
        /// Signs the in with email asynchronous.
        /// </summary>        
        /// <param name="inputModel">The sign in parameters.</param>
        /// <returns>The result of signing in with email and password.</returns>
        /// <response code="200">Succeeded.</response>
        /// <response code="202">Two-factor authentication requires.</response>
        /// <response code="401">Failed to login with given name and password or account was deleted but can be restored.</response>
        [HttpPost("/iam/api/v1/accounts/signin/password")]
        public async Task<ActionResult> SignInWithEmailAsync([FromBody] SignInWithEmailParams inputModel)
        {
            var user = await _accountRepo.Value.GetUserForNamePassword(inputModel.EmailOrUserName, inputModel.Password);

            var result = await _signInManager.CreyPasswordSignInAsync(user, inputModel.Password, true, lockoutOnFailure: false);
            if (result.SignInResult.Succeeded)
            {
                return Ok();
            }
            if (result.SignInResult.RequiresTwoFactor)
            {
                return Ok("Two-factor authentication requires.");
            }
            if (result.CanBeRestored)
            {
                await _signInManager.SignOutAsync();

                throw new HttpStatusErrorException(
                    System.Net.HttpStatusCode.Unauthorized,
                    "Account was deleted and can be restored.",
                    result.CanBeRestoredDetail);
            }

            throw new AccessDeniedException("Failed to login with given name and password.");
        }

        /// <summary>
        /// Impersonate server as user to allow get him user validity and roles from cookie.
        /// </summary>
        [HttpPost("/iam/s2s/accounts/{accountId}/signin")]
        //[ServerToServer]
        public async Task<SessionInfo> SignInWithAccountIdAsync(
            int accountId,
            [FromServices] ICreyService<AccountRepository> db,
            [FromServices] SignInManager<ApplicationUser> signIn,
            [FromServices] IIDInfoAccessor idInfo)
        {
            var user = await db.Value.FindUserByAccountIdAsync(accountId);
            await signIn.SignInAsync(user, false, CredentialType.Impersonation.ToString());
            // note: also session is passed in cookies, still simplify code with returning it directly
            // and in future seems it will be resonable somehow create `session` without setting cookies
            return idInfo.GetSessionInfo();
        }

        [HttpGet("/iam/s2s/accounts/{accountId}/roles")]
        [ServerToServer]
        public async Task<UserInfo> GetRolesAsync(
            int accountId,
            [FromServices] ICreyService<AccountRepository> db)
        {
            var roles = await db.Value
                                 .GetRolesSetAsync(accountId);
            return new UserInfo { AccountId = accountId, Roles = roles.ToHashSet() };
        }

        /// <summary>
        ///  Sign in with external provider.
        /// </summary>
        /// <param name="provider">Name of provider</param>
        /// <returns>The created Microsoft.AspNetCore.Mvc.ChallengeResult for the response.</returns>
        [HttpGet("/iam/api/v1/accounts/externallogin")]
        public async Task<IActionResult> SignInWithExternalProvider(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            if (provider == null) 
            {
                return BadRequest("The name of the provider is not specified");
            }

            var redirectUrl = Url.Action(nameof(HandleExternalLogin));
            var authenticationProperties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(authenticationProperties, provider);
        }

        /// <summary>
        /// Handle external login. Processing of new user data.
        /// </summary>
        /// <returns>Result of external login process.</returns>
        /// <response code="200">User logged in successfully.</response>
        /// <response code="202">Need external link account.</response>
        /// <response code="400">Some errors with external login information or user is locked out.</response>
        /// <response code="401">Account was deleted and can be restored.</response>
        /// <response code="404">Need to register new user.</response>
        [HttpGet("/iam/api/v1/accounts/externallogin/callback")]
        public async Task<IActionResult> HandleExternalLogin()
        {
            // info not null if Identity.External cookie exists
            var info = await _signInManager.GetExternalLoginInfoAsync() ??
                throw new CommandErrorException<NoData>("Error loading external login information.");

            // check if the user already exists in our system
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new CommandErrorException<NoData>($"Make sure you have access to your { info.LoginProvider } account or try to choose a different option.");
            }

            // try to login in our system
            var result = await _signInManager.CreyExternalLoginSignInAsync(email, info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.SignInResult.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                return Ok("User logged in successfully.");
            }
            if (result.SignInResult.IsLockedOut)
            {
                throw new CommandErrorException<NoData>("This account has been locked out, please try again later.");
            }
            if (result.CanBeRestored)
            {
                await _signInManager.SignOutAsync();

            if (result.SignInResult.RequiresTwoFactor) 
            {
                Ok("Need to be two-factor authentication");
            }

            var userHasEmail = info.Principal.HasClaim(c => c.Type == ClaimTypes.Email);

                throw new HttpStatusErrorException(
                    System.Net.HttpStatusCode.Unauthorized,
                    "Account was deleted and can be restored.",
                    result.CanBeRestoredDetail);
            }

            var user = await _userManager.FindByEmailAsync(email);            
            if (user == null)
            {
                // need to register new user in our system - Register flow
                var externalLoginModel = new ExternalLoginModel
                {
                    LoginProvider = info.LoginProvider,
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                    Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                };

                throw new HttpStatusErrorException(
                    System.Net.HttpStatusCode.NotFound,
                    "Need to register new user in our system.",
                    externalLoginModel);
            }

            // user returned back, use ExternalLinkAccount
            var userRetuned = $"This user is back and he is using the provider - {info.LoginProvider}." +
            $"Need confirm if he want to associate his {info.LoginProvider} account with the Crey account using the same email address.";

            return Accepted(userRetuned);
        }

        /// <summary>
        /// Link external account with account in our system and sign in.
        /// </summary>
        /// <returns>The action result</returns>
        [HttpPost("/iam/api/v1/accounts/externallogin/linkaccount")]
        public async Task<IActionResult> ExternalLinkAccount()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var errorMessage = "Error loading external login information.";

            if (info == null)
            {
                return BadRequest(errorMessage);
            }

            //add user which came back into our system
            ApplicationUser user = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
            var identityResult = await _userManager.AddLoginAsync(user, info);

            if (identityResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);

                return Ok("The current user is authenticated.");
            }

            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
    }
}