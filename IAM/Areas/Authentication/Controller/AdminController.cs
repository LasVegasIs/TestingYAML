using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using IAM.Contracts;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class ClassifiedUserSearchResultItem
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class ClassifiedUserInfo
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? LastLoginTime { get; set; }
        public List<string> RawRoles { get; set; }
    }


    public class ClassifiedUserSearchResult : PagedListResult<ClassifiedUserInfo>
    {
        [Obsolete("Use Items")]
        public List<ClassifiedUserInfo> Users => base.Items;
    }

    public class SetRolesParam
    {
        public List<string> Roles { get; set; }
    }


    public class SearchByParam
    {
        public string Keyword { get; set; }
        public string ContinuationToken { get; set; }
    }

    [ApiController]
    [EnableCors]
    [AuthenticateStrict]
    [Authorize(Policy = CreyAuthorizationDefaults.CreyUser)]
    public class AdminController : ControllerBase
    {
        private readonly ICreyService<AccountRepository> accountRepository_;

        public AdminController(ICreyService<AccountRepository> accountRepository)
        {
            accountRepository_ = accountRepository;
        }

        [HttpPut("/iam/adminapi/v1/roles/{role}")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public Task CreateRoleAsync(string role)
        {
            return accountRepository_.Value.CreateRoleAsync(role);
        }

        [HttpGet("/iam/adminapi/v1/roles")]
        [Authorize(Roles = UserRoles.UserAdmin + "," + Roles.FeatureManager)]
        public async Task<ActionResult<List<string>>> ListRolesAsync()
        {
            return await accountRepository_.Value.ListRolesAsync();
        }

        [HttpDelete("/iam/adminapi/v1/roles/{role}")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public Task DeleteRoleAsync(string role)
        {
            return accountRepository_.Value.DeleteRoleAsync(role);
        }

        [HttpGet("/iam/adminapi/v1/accounts/{userId}")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<ClassifiedUserInfo>> GetClassifiedUserById(int userId)
        {
            return await accountRepository_.Value.GetClassifiedUserByIdAsync(userId);
        }

        [HttpDelete("/iam/adminapi/v1/accounts/{accountId}")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task DeleteAccount(int accountId)
        {
            var user = await accountRepository_.Value.FindUserByAccountIdAsync(accountId)
                ?? throw new ItemNotFoundException($"No user exists for account ID {accountId}");

            await accountRepository_.Value.AdminHardDeleteAccountAsync(accountId);
        }

        [HttpPost("/iam/adminapi/v1/accounts/search/byname")]
        [Obsolete("use /iam/adminapi/v2/accounts/search/byname instead due to perf issues")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public Task<ClassifiedUserSearchResult> DeprecatedFindUserByName([FromBody] SearchByParam searchParam)
        {
            return accountRepository_.Value
                    .DeprecatedFindClassifiedUsersByNameAsync(searchParam.Keyword, searchParam.ContinuationToken)
                    .ToAsync(x => new ClassifiedUserSearchResult { Items = x.Items, ContinuationToken = x.ContinuationToken });
        }

        [HttpPost("/iam/adminapi/v2/accounts/search/byname")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public Task<PagedListResult<ClassifiedUserSearchResultItem>> FindUserByNameAsync([FromBody] SearchByParam searchParam)
        {
            return accountRepository_.Value.FindClassifiedUsersByNameAsync(searchParam.Keyword, searchParam.ContinuationToken);
        }

        [HttpPost("/iam/adminapi/v1/accounts/search/byemail")]
        [Obsolete("use /iam/adminapi/v2/accounts/search/byemail instead due to perf issues")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<ClassifiedUserSearchResult>> FindUserByEmail([FromBody] SearchByParam searchParam)
        {
            return await accountRepository_.Value
                                           .FindClassifiedUsersByEmailAsync(searchParam.Keyword, searchParam.ContinuationToken)
                                           .ToAsync(x => new ClassifiedUserSearchResult { Items = x.Items, ContinuationToken = x.ContinuationToken });
        }

        [HttpPost("/iam/adminapi/v2/accounts/search/byemail")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public Task<PagedListResult<ClassifiedUserSearchResultItem>> FindUserByEmail2([FromBody] SearchByParam searchParam)
        {
            return accountRepository_
                .Value
                .FindClassifiedUsersByEmailAsync2(searchParam.Keyword, searchParam.ContinuationToken);
        }

        [HttpPost("/iam/adminapi/v1/accounts/search/byrole")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<ClassifiedUserSearchResult>> FindUserByRoleAsync([FromBody] SearchByParam searchParam)
        {
            var roleList = searchParam.Keyword.Split(",").Select(role => role.Trim()).Distinct();
            return await accountRepository_.Value
                                           .FindClassifiedUsersByRoleAsync(roleList, searchParam.ContinuationToken)
                                           .ToAsync(x => new ClassifiedUserSearchResult { Items = x.Items, ContinuationToken = x.ContinuationToken });
        }

        [HttpGet("/iam/adminapi/v1/accounts/{accountId}/roles")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<List<string>>> GetRolesAsync(int accountId)
        {
            return await accountRepository_.Value.GetRolesSetAsync(accountId);
        }

        [HttpPut("/iam/adminapi/v1/accounts/{accountId}/roles/{role}")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<List<string>>> AddRoleAsync(int accountId, string role)
        {
            return await accountRepository_.Value.AddRoleAsync(accountId, role);
        }

        [HttpDelete("/iam/adminapi/v1/accounts/{accountId}/roles/{role}")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<List<string>>> RemoveRoleAsync(int accountId, string role)
        {
            return await accountRepository_.Value.RemoveRoleAsync(accountId, role);
        }

        [HttpPut("/iam/adminapi/v1/accounts/{accountId}/roles/set")]
        [Authorize(Roles = UserRoles.UserAdmin)]
        public async Task<ActionResult<List<string>>> SetRolesAsync(int accountId, [FromBody] SetRolesParam param)
        {
            return await accountRepository_.Value.SetRolesAsync(accountId, param.Roles);
        }

        [HttpPut("/iam/admin/api/v1/accounts/{accountId}/mute")]
        [Authorize(Roles = Roles.UserModerator)]
        public Task MuteUserAsync(int accountId)
        {
            return accountRepository_.Value.AddRoleAsync(accountId, UserRoles.Muted);
        }

        [HttpPut("/iam/admin/api/v1/accounts/{accountId}/unmute")]
        [Authorize(Roles = Roles.UserModerator)]
        public Task UnMuteUserAsync(int accountId)
        {
            return accountRepository_.Value.RemoveRoleAsync(accountId, UserRoles.Muted);
        }
    }
}