using Crey.Exceptions;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using IAM.Areas.Authentication.Models;
using IAM.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class CreySignInManager : SignInManager<ApplicationUser>
    {
        private readonly ICreyService<SessionRepository> sessionRepository_;
        private readonly ICreyService<AccountRepository> accountRepository_;
        private readonly IIDInfoAccessor idInfo_;
        private readonly UserManager<ApplicationUser> userManager_;

        public CreySignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation,
            ICreyService<SessionRepository> sessionRepository,
            ICreyService<AccountRepository> accountRepository,
            IIDInfoAccessor idInfo)
            : base(
                userManager,
                contextAccessor,
                claimsFactory,
                optionsAccessor,
                logger,
                schemes,
                confirmation)
        {
            sessionRepository_ = sessionRepository;
            accountRepository_ = accountRepository;
            idInfo_ = idInfo;
            userManager_ = userManager;
        }

        public async Task SignInAsync(int accountId, bool isPersistent, string authenticationMethod = null)
        {
            var applicationUser = await accountRepository_.Value.FindUserByAccountIdAsync(accountId);
            if (applicationUser == null)
            {
                throw new AccountNotFoundException($"No user with Account ID: {accountId}");
            }

            await SignInAsync(applicationUser, isPersistent, authenticationMethod);
        }

        public override async Task SignOutAsync()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            await sessionRepository_.Value.DeleteSession(sessionInfo);
            await base.SignOutAsync();
        }

        public async Task<CreySignInResult> CreyExternalLoginSignInAsync(string email, string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            var user = await userManager_.FindByEmailAsync(email);
            if (user == null)
            {
                return new CreySignInResult
                {
                    SignInResult = SignInResult.Failed
                };
            }
                        
            var signInResult = await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);

            (var canBeRestored, var canBeRestoredDetail) = await UserCanBeRestored(user);
            if (canBeRestored)
            {
                return new CreySignInResult
                {
                    SignInResult = SignInResult.Failed,
                    CanBeRestored = canBeRestored,
                    CanBeRestoredDetail = canBeRestoredDetail
                };
            }

            return new CreySignInResult
            {
                SignInResult = signInResult
            };
        }        

        public async Task<CreySignInResult> CreyPasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var signInResult = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

            (var canBeRestored, var canBeRestoredDetail) = await UserCanBeRestored(user);
            if (canBeRestored)
            {
                return new CreySignInResult
                {
                    SignInResult = SignInResult.Failed,
                    CanBeRestored = canBeRestored,
                    CanBeRestoredDetail = canBeRestoredDetail
                };
            }

            return new CreySignInResult
            {
                SignInResult = signInResult
            };
        }

        private async Task<(bool, CanBeRestoredDetail)> UserCanBeRestored(ApplicationUser user)
        {
            if (!await accountRepository_.Value.IsUserSoftDeletedAsync(user.AccountId))
            {
                return (false, null);
            }

            var requirePassword = await userManager_.HasPasswordAsync(user);
            var detail = new CanBeRestoredDetail
            {
                UserId = user.Id,
                CanBeRestored = true,
                RequirePassword = requirePassword
            };

            return (true, detail);
        }
    }
}