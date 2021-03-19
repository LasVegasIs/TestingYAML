using Core.Extensions;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.MessageStream.ServiceBus;
using Crey.QueriableExtensions;
using IAM.Areas.Authentication.ServiceBusMessages;
using IAM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IAM.Areas.Authentication.EmailController;

namespace IAM.Areas.Authentication
{
    public class AccountRepository
    {
        private readonly ApplicationDbContext appDBContext_;
        private readonly ILogger logger_;
        private readonly IConfiguration configuration_;

        private readonly UserManager<ApplicationUser> userManager_;
        private readonly RoleManager<IdentityRole> roleManager_;
        private readonly ICreyService<SessionTokenRepository> sessionTokenRepository_;
        private readonly CreyRestClient creyRestClient_;
        private readonly IIDInfoAccessor idInfo_;
        private readonly CreyMessageBroker<IAccountServiceBusMessage> accountMessageBroker_;

        public AccountRepository(
            ILogger<AccountRepository> logger,
            IConfiguration configuration,
            ApplicationDbContext appDBContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICreyService<SessionTokenRepository> sessionTokenRepository,
            CreyRestClient creyRestClient,
            IIDInfoAccessor idInfo,
            CreyMessageBroker<IAccountServiceBusMessage> accountMessageBroker)
        {
            logger_ = logger;
            configuration_ = configuration;
            appDBContext_ = appDBContext;
            userManager_ = userManager;
            roleManager_ = roleManager;
            sessionTokenRepository_ = sessionTokenRepository;
            creyRestClient_ = creyRestClient;
            idInfo_ = idInfo;
            accountMessageBroker_ = accountMessageBroker;
        }

        public async Task<string> GetDefaultDisplayName(int accountId)
        {
            var user = await FindUserByAccountIdAsync(accountId)
                 ?? throw new ItemNotFoundException($"No user exists for account ID {accountId}");
            return user.UserName;

        }

        public async Task<bool> GetEmailConfirmedStatusAsync(int accountId)
        {
            var user = await FindUserByAccountIdAsync(accountId)
                 ?? throw new ItemNotFoundException($"No user exists for account ID {accountId}");
            return user.EmailConfirmed;
        }

        public async Task<EmailStatus> GetEmailStatusAsync(int accountId)
        {
            var user = await FindUserByAccountIdAsync(accountId)
                 ?? throw new ItemNotFoundException($"No user exists for account ID {accountId}");

            return new EmailStatus
            {
                Confirmed = user.EmailConfirmed,
                Newsletter = user.NewsletterSubscribed
            };
        }

        public async Task SetEmailStatusAsync(int accountId, PatchEmailStatus param)
        {
            //early exit, if PatchEmail gets more attributes (ex email about notifications), it have to be refactored
            if (!param.Newsletter.HasValue)
                return;

            var user = await FindUserByAccountIdAsync(accountId)
                 ?? throw new ItemNotFoundException($"No user exists for account ID {accountId}");

            if (user.NewsletterSubscribed == param.Newsletter)
                return;
            user.NewsletterSubscribed = param.Newsletter.Value;

            await appDBContext_.SaveChangesAsync();
        }

        public Task CreateRoleAsync(string role)
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            if (!sessionInfo.IsUser)
                throw new AccessDeniedException("Login required");
            if (!sessionInfo.Roles.Contains(UserRoles.UserAdmin))
                throw new AccessDeniedException("Insufficient roles");
            if (UserRoles.PhantomRoles.Contains(role))
                throw new InvalidArgumentException($"phantom role cannot be created: {role}");
            return roleManager_.CreateAsync(new IdentityRole(role));
        }

        public async Task DeleteRoleAsync(string role)
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            if (!sessionInfo.IsUser)
                throw new AccessDeniedException("Login required");
            if (!sessionInfo.Roles.Contains(UserRoles.UserAdmin))
                throw new AccessDeniedException("Insufficient roles");

            var identityRole = (await roleManager_.FindByNameAsync(role))
                ?? throw new ItemNotFoundException($"Role {role} does not exists");
            await roleManager_.DeleteAsync(identityRole);
        }

        public Task<List<string>> ListRolesAsync()
        {
            return roleManager_.Roles
                .Select(x => x.Name)
                .Where(x => !UserRoles.PhantomRoles.Contains(x))
                .ToListAsync();
        }

        public async Task CancelSoftDelete(int accountId)
        {
            var deletedUser = await appDBContext_.SoftDeletedUserAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (deletedUser == null)
            {
                return;
            }

            appDBContext_.SoftDeletedUserAccounts.Remove(deletedUser);
            await appDBContext_.SaveChangesAsync();
        }

        public async Task SoftDeleteAccount(int accountId)
        {
            var softDeletedAccount = await appDBContext_.SoftDeletedUserAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (softDeletedAccount == null)
            {
                try
                {
                    await appDBContext_.SoftDeletedUserAccounts.AddAsync(new DBSoftDeletedUserAccount
                    {
                        AccountId = accountId,
                        TimeStamp = DateTime.UtcNow
                    });
                    await appDBContext_.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.IsConflictException())
                {
                    logger_.LogInformation($"Account {accountId} is already soft deleted");
                }
            }
            else
            {
                logger_.LogInformation($"Account {accountId} is already soft deleted");
            }

            bool revokedTokens;
            try
            {
                await sessionTokenRepository_.Value.RevokeAllTokensAsync(accountId);
                revokedTokens = true;
            }
            catch (Exception)
            {
                logger_.LogError($"Failed to revoke tokens for soft deleted user {accountId}");
                revokedTokens = false;
            }

            if (!revokedTokens)
            {
                throw new ServerErrorException($"Failed to soft delete account for user {accountId}");
            }
        }

        public async Task HardDeleteAccount(int accountId)
        {
            var hardDeletedUser = await appDBContext_.HardDeletedUserAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (hardDeletedUser == null)
            {
                try
                {
                    await appDBContext_.HardDeletedUserAccounts.AddAsync(new DBHardDeletedUserAccount
                    {
                        AccountId = accountId,
                        TimeStamp = DateTime.UtcNow
                    });
                    await appDBContext_.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.IsConflictException())
                {
                    logger_.LogInformation($"Account {accountId} is already hard deleted");
                }
            }
            else
            {
                logger_.LogInformation($"Account {accountId} is already hard deleted");
            }

            bool userDeleted = await DeletePersonallyIdentifiableInformationAsync(accountId);

            bool dataRemoved;
            try
            {
                await DeleteUserDataAsync(accountId);
                dataRemoved = true;
            }
            catch (Exception)
            {
                logger_.LogError($"Failed to remove user data for {accountId}");
                dataRemoved = false;
            }

            bool revokedTokens;
            try
            {
                await sessionTokenRepository_.Value.RevokeAllTokensAsync(accountId);
                revokedTokens = true;
            }
            catch (Exception)
            {
                logger_.LogError($"Failed to revoke tokens for hard deleted user {accountId}");
                revokedTokens = false;
            }

            bool tokensPseudonymizedTokens;
            try
            {
                await sessionTokenRepository_.Value.RemovePersonallyIdentifiableInformationFromTokensAsync(accountId);
                tokensPseudonymizedTokens = true;
            }
            catch (Exception)
            {
                logger_.LogError($"Failed to pseudonymize tokens for hard deleted user {accountId}");
                tokensPseudonymizedTokens = false;
            }

            if (!(userDeleted && revokedTokens && tokensPseudonymizedTokens && dataRemoved))
            {
                throw new ServerErrorException($"Failed to hard delete account for user {accountId}");
            }
        }

        public Task AdminHardDeleteAccountAsync(int accountId)
        {
            return accountMessageBroker_.SendMessages(new IAccountServiceBusMessage[]
            {
                new SoftDeleteServiceBusMessage { AccountId = accountId },
                new HardDeleteServiceBusMessage { AccountId = accountId }
            });
        }

        private async Task<bool> DeletePersonallyIdentifiableInformationAsync(int accountId)
        {
            var applicationUser = await FindUserByAccountIdAsync(accountId);
            if (applicationUser == null)
            {
                return true;
            }

            bool userNameRemoved = (await userManager_.SetUserNameAsync(applicationUser, $"DELETED-{accountId}")).Succeeded;
            if (!userNameRemoved)
            {
                logger_.LogError($"Failed to remove UserName for user {accountId}");
            }

            bool emailRemoved = (await userManager_.SetEmailAsync(applicationUser, $"DELETED-{accountId}@DELETED.ACCOUNT")).Succeeded;
            if (!emailRemoved)
            {
                logger_.LogError($"Failed to remove Email for user {accountId}");
            }

            bool phoneNumberRemoved = (await userManager_.SetPhoneNumberAsync(applicationUser, "")).Succeeded;
            if (!phoneNumberRemoved)
            {
                logger_.LogError($"Failed to remove PhoneNumber for user {accountId}");
            }

            bool passwordRemoved = (await userManager_.RemovePasswordAsync(applicationUser)).Succeeded;
            if (!passwordRemoved)
            {
                logger_.LogError($"Failed to remove Password for user {accountId}");
            }

            bool rolesRemoved = true;
            var roles = await userManager_.GetRolesAsync(applicationUser);
            if (roles.Any())
            {
                rolesRemoved = (await userManager_.RemoveFromRolesAsync(applicationUser, roles)).Succeeded;
                if (!rolesRemoved)
                {
                    logger_.LogError($"Failed to remove Roles for user {accountId}");
                }
            }

            bool loginsRemoved = true;
            var logins = await userManager_.GetLoginsAsync(applicationUser);
            if (logins.Any())
            {
                foreach (var login in logins)
                {
                    loginsRemoved &= (await userManager_.RemoveLoginAsync(applicationUser, login.LoginProvider, login.ProviderKey)).Succeeded;
                }

                if (!loginsRemoved)
                {
                    logger_.LogError($"Failed to remove External Logins for user {accountId}");
                }
            }

            return
                userNameRemoved
                && emailRemoved
                && phoneNumberRemoved
                && passwordRemoved
                && rolesRemoved
                && loginsRemoved;
        }

        public async Task HardDeleteAccountsAsync()
        {
            string continuationToken = null;

            do
            {
                var usersToDelete = await GetUsersToHardDeleteAsync(continuationToken);
                continuationToken = usersToDelete.ContinuationToken;

                var messages = usersToDelete.Items.Select(x => new HardDeleteServiceBusMessage { AccountId = x.AccountId });
                await accountMessageBroker_.SendMessages(messages);
            }
            while (continuationToken != null);
        }

        public async Task ReDeleteSoftDeletedAccountsAsync()
        {
            string continuationToken = null;

            do
            {
                var usersToDelete = await GetSoftDeletedUsers(continuationToken);
                continuationToken = usersToDelete.ContinuationToken;

                var messages = usersToDelete.Items.Select(x => new SoftDeleteServiceBusMessage { AccountId = x.AccountId });
                await accountMessageBroker_.SendMessages(messages);
            }
            while (continuationToken != null);
        }

        public async Task ReDeleteHardDeletedAccountsAsync()
        {
            string continuationToken = null;

            do
            {
                var usersToDelete = await GetHardDeletedUsers(continuationToken);
                continuationToken = usersToDelete.ContinuationToken;

                var messages = usersToDelete.Items.Select(x => new HardDeleteServiceBusMessage { AccountId = x.AccountId });
                await accountMessageBroker_.SendMessages(messages);
            }
            while (continuationToken != null);
        }

        private Task<PagedListResult<DBSoftDeletedUserAccount>> GetUsersToHardDeleteAsync(string continuationToken)
        {
            var hardDeletionDelay = configuration_.IsProductionSlot() ? TimeSpan.FromDays(7) : TimeSpan.FromMinutes(5);
            var threshold = DateTime.UtcNow.Subtract(hardDeletionDelay);

            var query = from dbDeletedUser in appDBContext_.SoftDeletedUserAccounts
                        where dbDeletedUser.TimeStamp < threshold
                        orderby dbDeletedUser.TimeStamp ascending
                        select dbDeletedUser;

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x);
        }

        private Task<PagedListResult<DBSoftDeletedUserAccount>> GetSoftDeletedUsers(string continuationToken)
        {
            var query = from dbDeletedUser in appDBContext_.SoftDeletedUserAccounts
                        orderby dbDeletedUser.TimeStamp ascending
                        select dbDeletedUser;

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x);
        }

        private Task<PagedListResult<DBHardDeletedUserAccount>> GetHardDeletedUsers(string continuationToken)
        {
            var query = from dbDeletedUser in appDBContext_.HardDeletedUserAccounts
                        orderby dbDeletedUser.TimeStamp ascending
                        select dbDeletedUser;

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x);
        }

        public async Task<ClassifiedUserInfo> GetClassifiedUserByIdAsync(int accountId)
        {
            var user = await appDBContext_.UserSearch()
                .FirstOrDefaultAsync(x => x.AccountId == accountId)
                ?? throw new ItemNotFoundException($"User {accountId} not found");

            return user.ToClassifiedUserInfo();
        }

        public Task<PagedListResult<ClassifiedUserInfo>> DeprecatedFindClassifiedUsersByNameAsync(string name, string continuationToken)
        {
            var query = appDBContext_.UserSearch()
                .Where(x => x.NormalizedName.Contains(userManager_.NormalizeName(name)))
                .OrderBy(x => x.NormalizedName);

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x.ToClassifiedUserInfo());
        }

        public Task<PagedListResult<ClassifiedUserSearchResultItem>> FindClassifiedUsersByNameAsync(string name, string continuationToken)
        {
            var query = from user in appDBContext_.Users
                        where user.NormalizedUserName.Contains(userManager_.NormalizeName(name))
                        orderby user.NormalizedUserName
                        select user;

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x.ToClassifiedUserSearchResultItem());
        }

        public Task<PagedListResult<ClassifiedUserInfo>> FindClassifiedUsersByEmailAsync(string email, string continuationToken)
        {
            var query = appDBContext_.UserSearch()
                .Where(x => x.Email.Contains(email))
                .OrderBy(x => x.AccountId);

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x.ToClassifiedUserInfo());
        }

        public Task<PagedListResult<ClassifiedUserSearchResultItem>> FindClassifiedUsersByEmailAsync2(string email, string continuationToken)
        {
            var query = appDBContext_.Users
                .Where(x => x.NormalizedEmail.Contains(userManager_.NormalizeEmail(email)))
                .OrderBy(x => x.NormalizedEmail);

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x.ToClassifiedUserSearchResultItem());
        }

        public Task<PagedListResult<ClassifiedUserInfo>> FindClassifiedUsersByRoleAsync(IEnumerable<string> roleList, string continuationToken)
        {
            if (!roleList.Any()) throw new ArgumentException("Should provide roleList", nameof(roleList));

            var query = appDBContext_.UserSearch();
            foreach (var r in roleList)
            {
                query = query.Where(x => x.Roles.Contains(r));
            }
            query = query.OrderBy(x => x.AccountId);

            var token = new OffsetBasedContinuationToken(continuationToken);
            return query.PaginateListResultAsync(token, 20, x => x.ToClassifiedUserInfo());
        }

        public async Task<List<string>> GetRolesSetAsync(int accountId)
        {
            ApplicationUser user = await FindUserByAccountIdAsync(accountId)
                ?? throw new ItemNotFoundException($"Account {accountId} not found");
            return await GetRolesAsync(user);
        }

        public async Task<List<string>> AddRoleAsync(int accountId, string role)
        {
            ApplicationUser user = await FindUserByAccountIdAsync(accountId)
                ?? throw new ItemNotFoundException($"Account {accountId} not found");
            var currentRoles = await userManager_.GetRolesAsync(user);
            if (currentRoles.Contains(role))
                return currentRoles as List<string>;

            await userManager_.AddToRoleAsync(user, role);
            return await GetRolesAsync(user);
        }

        public async Task<List<string>> RemoveRoleAsync(int accountId, string role)
        {
            ApplicationUser user = await FindUserByAccountIdAsync(accountId)
                ?? throw new ItemNotFoundException($"Account {accountId} not found");
            var currentRoles = await userManager_.GetRolesAsync(user);
            if (!currentRoles.Contains(role))
                return currentRoles as List<string>;

            await userManager_.RemoveFromRoleAsync(user, role);
            return await GetRolesAsync(user);
        }

        public async Task<List<string>> SetRolesAsync(int accountId, List<string> roles)
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            if (!sessionInfo.IsUser)
                throw new AccessDeniedException("Login required");
            if (!sessionInfo.Roles.Contains(UserRoles.UserAdmin))
                throw new AccessDeniedException("Insufficient roles");

            var rolesToAdd = roles.Where(x => !UserRoles.PhantomRoles.Contains(x)).ToList();

            var user = await FindUserByAccountIdAsync(accountId)
                ?? throw new ItemNotFoundException($"Account {accountId} not found");

            var currentRoles = (await userManager_.GetRolesAsync(user)).ToHashSet();
            currentRoles.ExceptWith(rolesToAdd);
            if (currentRoles.Any())
                await userManager_.RemoveFromRolesAsync(user, currentRoles);
            foreach (var role in rolesToAdd)
            {
                await userManager_.AddToRoleAsync(user, role);
            }

            return await GetRolesAsync(user);
        }

        public async Task<List<string>> GetRolesAsync(ApplicationUser user)
        {
            var roles = (await userManager_.GetRolesAsync(user)).ToHashSet();

            roles.Remove(UserRoles.InternalUser);
            if (IsInternalUser(user))
                roles.Add(UserRoles.InternalUser);

            if (configuration_.IsProductionSlot())
            {
                roles.Remove("Dev");
                roles.Remove(UserRoles.ExternalDeveloper); // sry, no extra privilage in production
            }
            else
            {
                if (roles.Contains(UserRoles.InternalUser) || roles.Contains(UserRoles.ExternalDeveloper))
                    roles.Add(UserRoles.UserAdmin);
            }

            // by default everybody gets the FreeUser role
            if (!roles.Contains(UserRoles.FreeUser))
                roles.Add(UserRoles.FreeUser);

            return roles.ToList();
        }

        public async Task<bool> IsUserDeletedAsync(int accountId)
        {
            var softDeletedUser = await appDBContext_.SoftDeletedUserAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (softDeletedUser != null)
            {
                return true;
            }

            var hardDeletedUser = await appDBContext_.HardDeletedUserAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (hardDeletedUser != null)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsUserSoftDeletedAsync(int accountId)
        {
            return (await appDBContext_.SoftDeletedUserAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId)) != null;            
        }

        public Task CreateUserDataAsync(int accountId, DateTime utcDateOfBirth)
        {
            appDBContext_.Add(new DBUserData
            {
                AccountId = accountId,
                DateOfBirth = utcDateOfBirth,
            });
            return appDBContext_.SaveChangesAsync();
        }

        public async Task<DBUserData> GetUserDataAsync(int accountId)
        {
            var result = await (from userData in appDBContext_.UserDatas
                                where userData.AccountId == accountId
                                select userData).FirstOrDefaultAsync();

            return result;
        }

        public Task DeleteUserDataAsync(int accountId)
        {
            var userData = appDBContext_.UserDatas.FirstOrDefault(ud => ud.AccountId == accountId);
            if (userData == null)
            {
                return Task.CompletedTask;
            }

            appDBContext_.Remove(userData);
            return appDBContext_.SaveChangesAsync();
        }

        public async Task<ApplicationUser> GetUserForNamePassword(string emailOrUserName, string password)
        {
            var user =
                   await userManager_.FindByEmailAsync(emailOrUserName)
                ?? await userManager_.FindByNameAsync(emailOrUserName)
                ?? throw new AccessDeniedException("User or password is not valid.");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new InvalidArgumentException("User does not have a password");
            }

            if ((await GetRolesAsync(user)).Contains("InternalUser") && !isPassStrongForInternalUser(password))
            {
                throw new PasswordException("Reset your password");
            }
            return user;
        }        

        private bool isPassStrongForInternalUser(string password)
        {
            var deploymentSlot = configuration_.GetDeploymentSlot();
            if (deploymentSlot == "dev")
            {
                return true;
            }

            int score = 0;

            string di = @"\d+"; //match digits
            string upp = @"[A-Z]+"; //match upper cases
            string low = @"[a-z]+"; //match lower cases
            string sym = @"\W|_"; //match symbols

            if (Regex.Match(password, di).Success)
                score++;
            if (Regex.Match(password, upp).Success)
                score++;
            if (Regex.Match(password, low).Success)
                score++;
            if (Regex.Match(password, sym).Success)
                score++;

            if (password.Length >= 8 && score >= 3)
            {
                return true;
            }
            return false;
        }

        private bool IsInternalUser(ApplicationUser user)
        {
            bool internalEmail = user.NormalizedEmail.EndsWith("@CREYGAMES.COM") || user.NormalizedEmail.EndsWith("@BITGLOBE.HU") || user.NormalizedEmail.EndsWith("@BITGLOBE.COM");
            if (user.EmailConfirmed && internalEmail)
            {
                return true;
            }

            // add other users by id, etc.
            return false;
        }

        public Task<ApplicationUser> FindUserByAccountIdAsync(int accountId)
        {
            return (from user in appDBContext_.Users
                    where user.AccountId == accountId
                    select user)
                    .FirstOrDefaultAsync();
        }
    }
}