using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel;
using Crey.Kernel.ServiceDiscovery;
using IAM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class SessionRepository
    {
        private readonly IConfiguration configuration_;

        private readonly ApplicationDbContext appDBContext_;
        private readonly ILogger logger_;

        private readonly UserManager<ApplicationUser> userManager_;
        private readonly ICreyService<SessionTokenRepository> sessionTokenRepository_;
        private readonly ICreyService<AccountRepository> accountRepository_;
        private readonly CreyRestClient creyRestClient_;

        public SessionRepository(
            ILogger<SessionRepository> logger,
            IConfiguration configuration,
            ApplicationDbContext appDBContext,
            UserManager<ApplicationUser> userManager,
            ICreyService<SessionTokenRepository> sessionTokenRepository,
            ICreyService<AccountRepository> accountRepository,
            CreyRestClient creyRestClient)
        {
            logger_ = logger;
            configuration_ = configuration;
            appDBContext_ = appDBContext;
            userManager_ = userManager;
            sessionTokenRepository_ = sessionTokenRepository;
            creyRestClient_ = creyRestClient;
            accountRepository_ = accountRepository;
        }

        public async Task<SessionInfo> CreateNewSessionForIdentityUser(string userId, SiteInfo siteInfo, CredentialType credentialType)
        {
            var user = await userManager_.FindByIdAsync(userId);
            if (user == null)
            {
                throw new AccountNotFoundException($"No user with Identity ID {userId}");
            }

            var token = await sessionTokenRepository_.Value.CreateToken(user.AccountId, siteInfo, credentialType);
            return await CreateSessionForIdentityUser(user, token, credentialType);
        }

        public async Task<SessionInfo> RefreshSessionForIdentityUser(string userId, SiteInfo siteInfo)
        {
            var user = await userManager_.FindByIdAsync(userId);
            if (user == null)
            {
                throw new AccountNotFoundException($"No user with Identity ID {userId}");
            }

            var sessionToken = await sessionTokenRepository_.Value.FindSessionTokenByUser(user.AccountId, siteInfo);
            return await CreateSessionForIdentityUser(user, sessionToken.Token, sessionToken.CredentialType);
        }

        public async Task<SessionInfo> GetSessionInfoByToken(string token, SiteInfo siteInfo)
        {
            var session = await sessionTokenRepository_.Value.FindUserSessionByToken(token, siteInfo);
            var user = await accountRepository_.Value.FindUserByAccountIdAsync(session.AccountId);
            return await CreateSessionForIdentityUser(user, session.Token, session.Credential);
        }

        private async Task<SessionInfo> CreateSessionForIdentityUser(ApplicationUser user, string token, CredentialType credentialType)
        {
            var roles = await accountRepository_.Value.GetRolesAsync(user);
            var isDeleted = await accountRepository_.Value.IsUserDeletedAsync(user.AccountId);
            var sessionInfo = ToSessionInfo(user.AccountId, user.Id, token, roles, credentialType, isDeleted);

            logger_.LogInformation($"User logged in: {sessionInfo.AccountId}, key: {sessionInfo.Key}");
            return sessionInfo;
        }

        public async Task DeleteSession(SessionInfo sessionInfo)
        {
            logger_.LogInformation($"User logged out: {sessionInfo.AccountId}, key:{sessionInfo.Key}");

            if (sessionInfo.Key.StartsWith("2"))
            {
                await sessionTokenRepository_.Value.RevokeToken(sessionInfo.Key);
            }
            else
            {
                ClearToken(sessionInfo.Key);
            }
        }

        private SessionInfo ToSessionInfo(int accountId, string userId, string key, IEnumerable<string> roles, CredentialType credentialType, bool isDeleted)
        {
            return new SessionInfo
            {
                AccountId = accountId,
                UserId = userId,
                Key = key,
                Roles = roles.ToHashSet(),
                AuthenticationMethod = credentialType.ToString(),
                IsDeleted = isDeleted
            };
        }

        [Obsolete("Only kept for backwards compatibility")]
        private void ClearToken(string key)
        {
            if (key.StartsWith("1"))
            {
                var query = (from token in appDBContext_.AuthToken
                             where token.Token == key
                             select token)
                            .FirstOrDefault();

                if (query == null)
                    return;

                query.Token = "";
                appDBContext_.SaveChanges();
            }
        }
    }
}