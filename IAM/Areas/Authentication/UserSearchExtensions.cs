using Crey.Contracts;
using IAM.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public static class UserSearchExtensions
    {
        public static ClassifiedUserSearchResultItem ToClassifiedUserSearchResultItem(this ApplicationUser user)
        {
            return new ClassifiedUserSearchResultItem
            {
                AccountId = user.AccountId,
                Name = user.UserName,
                Email = user.Email
            };
        }

        public static ClassifiedUserInfo ToClassifiedUserInfo(this DBUserSearch user)
        {
            var roles = new List<string>();
            if (user.Roles != null)
                roles.AddRange(user.Roles.Split(','));

            return new ClassifiedUserInfo
            {
                AccountId = user.AccountId,
                Name = user.Name,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                CreationTime = user.CreationTime,
                LastLoginTime = user.LastLoginTime,
                RawRoles = roles
            };
        }

        public static IQueryable<DBUserSearch> UserSearch(this ApplicationDbContext applicationDb)
        {
            return applicationDb.Set<DBUserSearch>()
                .FromSqlRaw(@"
                    SELECT 
                        AspNetUsers.AccountId as AccountId,
                        AspNetUsers.UserName as Name, 
                        AspNetUsers.NormalizedUserName as NormalizedName, 
                        AspNetUsers.Email as Email, 
                        AspNetUsers.Creation as CreationTime,
                        AspNetUsers.EmailConfirmed as EmailConfirmed,
                        lt.LastLoginTime as LastLoginTime,
                        r.Roles as Roles
                    FROM AspNetUsers
                    LEFT JOIN (
                        select AccountId, MAX(LastLogin) as LastLoginTime
                        from AuthToken
                        group by AccountId
                    ) as lt ON lt.AccountId = AspNetUsers.AccountId
                    LEFT JOIN (
                        SELECT ur.UserId as UserId, STRING_AGG(AspNetRoles.Name, ',') as Roles
                        FROM  AspNetUserRoles ur
                        JOIN AspNetRoles ON ur.RoleId = AspNetRoles.Id
                        GROUP BY ur.UserId
                    ) as r ON r.UserId = AspNetUsers.Id
                ").AsNoTracking();
        }
    }
}
