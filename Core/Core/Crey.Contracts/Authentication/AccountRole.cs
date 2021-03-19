using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    [DataContract]
    [Flags]
    public enum AccountRoles : uint
    {
        [EnumMember] None = 0,                   // [METAROLE]
        //[EnumMember] Admin = 1 << 1,             
        [EnumMember] Dev = 1 << 2,
        [EnumMember] ContentDev = 1 << 3,
        [EnumMember] FreeUser = 1 << 4,           // [METAROLE] general user, default role for everybody who logged in
        [EnumMember] Anonymous = 1 << 5,          // not a real user, mask commands those can be executed without login
        [EnumMember] Banned = 1 << 6,             // banned user, shall get no other roles
        [EnumMember] Guest = 1 << 7,              // [METAROLE] guest user, restricted use only without an email-login
        [EnumMember] Subscriber = 1 << 8,
        [EnumMember] Tester = 1 << 9,

        [EnumMember] Moderator = 1 << 9,          // moderate user content
        [EnumMember] NoAnalytics = 1 << 10,       // not part of most analytics collection
        [EnumMember] Muted = 1 << 11,       // not part of most analytics collection
    }

    public static class AccountRolesExt
    {
        public static HashSet<string> ToRoleSet(this AccountRoles roles)
        {
            var res = new HashSet<string>();
            foreach (AccountRoles token in Enum.GetValues(typeof(AccountRoles)))
            {
                if (token == AccountRoles.None)
                    continue;

                if (roles.HasFlag(token))
                    res.Add(token.ToString());
            }

            return res;
        }

        public static AccountRoles ToAccountRolesMask(this HashSet<string> roles)
        {
            var res = (AccountRoles)0;
            foreach (string token in roles)
            {
                AccountRoles cur;
                if (Enum.TryParse(token, out cur))
                    res |= cur;
            }

            return res;
        }

        public static AccountRoles ToAccountRoles(this List<AccountRoles> roles)
        {
            var sum = AccountRoles.None;
            foreach (var accountRole in roles)
            {
                sum |= accountRole;
            }

            return sum;
        }

        public static bool CheckToMask(this AccountRoles role, AccountRoles mask)
        {
            if (mask.HasFlag(AccountRoles.Anonymous) || mask.HasFlag(AccountRoles.Banned))
            {
                // Commands with these roles are accessible for everyone (even for banned users)
                return true;
            }

            return !role.HasFlag(AccountRoles.Banned) && (role & mask) != 0;
        }
    }
}
