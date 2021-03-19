using Crey.Instrumentation.Web;
using Crey.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace Crey.Authentication
{
    public enum SystemGroups
    {
        Moderators = 1
    }

    // role constant for convenience, to convert typo into compile errors
    public static class UserRoles
    {
        public const string FreeUser = "FreeUser";
        public const string Anonymous = "Anonymous";
        public const string InternalUser = "InternalUser";
        public const string ExternalDeveloper = "ExternalDeveloper"; // outsourcing roles

        public const string ContentDev = "ContentDev";
        public const string Moderator = "Moderator";
        public const string Subscriber = "Subscriber";
        public const string DevStage = "DevStage";

        public const string Muted = "Muted";

        // roles those are not really roles and not present in the DB
        public static List<string> PhantomRoles = new List<string>
        {
            FreeUser, Anonymous, InternalUser
        };
    }

    /// <summary>
    /// Most usable part in service logic.
    /// </summary>
    public class UserInfo
    {
        public int AccountId { get; set; }

        public HashSet<string> Roles { get; set; } = new HashSet<string>();

        public bool IsDeleted { get; set; } = false;
    }

    public class SessionInfo : UserInfo
    {
        public string UserId { get; set; }

        public string Key { get; set; } = "";

        public string AuthenticationMethod { get; set; } = "";

        public bool IsValid => AccountId != 0;
        public bool IsSignedIn => !string.IsNullOrEmpty(Key);

        public bool IsUser => AccountId > 0;

        public List<int> GroupIds => Roles.Contains(UserRoles.Moderator) ? new List<int> { (int)SystemGroups.Moderators } : new List<int>();

        public SessionInfo Clone()
        {
            return new SessionInfo
            {
                AccountId = AccountId,
                UserId = UserId,
                Key = Key.Clone() as string,
                Roles = new HashSet<string>(Roles),
                AuthenticationMethod = AuthenticationMethod
            };
        }

        // Checks if toTest is valid with respect to this
        public bool CheckValidity(SessionInfo toTest)
        {
            if (AccountId != toTest.AccountId)
                return false;
            if (Key != toTest.Key)
                return false;

            return true;
        }
    }

    public static class SessionInfoSigner
    {
        public static string CreateSignedToken(SessionInfo sessionInfo, string secret)
        {
            string cookieData = JsonConvert.SerializeObject(sessionInfo);
            string cookieSignature = CreateSignature(cookieData, secret);
            return $"{cookieSignature};{cookieData}";
        }

        public static SessionInfo CreateFromSignedToken(string signedToken, string secret)
        {
            var cookieComponents = signedToken.Split(";", 2);
            if (cookieComponents.Length != 2)
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Malformed signedToken");
            }

            string cookieSignature = cookieComponents[0];
            string cookieData = cookieComponents[1];

            if (cookieSignature == null || cookieSignature == "")
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Missing signature");
            }

            var signature = CreateSignature(cookieData, secret);
            if (signature != cookieSignature)
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Signature mismatch");
            }

            var sessionInfo = JsonConvert.DeserializeObject<SessionInfo>(cookieData);
            if (sessionInfo == null)
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Malformed signedToken");
            }

            return sessionInfo;
        }

        private static string CreateSignature(string data, string secret)
        {
            Trace.Assert(!string.IsNullOrEmpty(data));
            return (secret == "") ? "" : CryptoHelper.CreyCryptoHash(data + secret);
        }
    }

    public static class SessionInfoExtensions
    {
        public static SessionInfo GetSessionInfo(this HttpContext context)
        {
            return context.User.IntoSessionInfo();
        }

        public static SessionInfo IntoSessionInfo(this ClaimsPrincipal principal)
        {
            // need to check for both authentication types because IAM must use IdentityConstants.ApplicationScheme
            ClaimsIdentity claimsIdentity = principal.GetSessionIdentity();
            if (claimsIdentity == null)
            {
                return new SessionInfo();
            }

            int.TryParse(claimsIdentity.FindFirst(claim => claim.Type == CreyClaimTypes.AccountId).Value, out int accountId);
            var keyClaim = claimsIdentity.FindFirst(claim => claim.Type == CreyClaimTypes.Key).Value;
            var isDeletedClaim = claimsIdentity.FindFirst(claim => claim.Type == CreyClaimTypes.IsDeleted);
            var userIdClaim = claimsIdentity.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier);
            var authenticationMethodClaim = claimsIdentity.FindFirst(claim => claim.Type == ClaimTypes.AuthenticationMethod).Value;

            var roles = new HashSet<string>();
            var roleClaims = claimsIdentity.FindAll(ClaimTypes.Role);
            foreach (var role in roleClaims)
            {
                roles.Add(role.Value);
            }

            return new SessionInfo
            {
                AccountId = accountId,
                UserId = userIdClaim == null ? null : userIdClaim.Value,
                Key = keyClaim,
                Roles = roles,
                AuthenticationMethod = authenticationMethodClaim,
                IsDeleted = isDeletedClaim == null ? false : Convert.ToBoolean(isDeletedClaim.Value),
            };
        }

        public static ClaimsIdentity GetSessionIdentity(this ClaimsPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = principal.Identities.FirstOrDefault(identity => identity.AuthenticationType == IdentityConstants.ApplicationScheme);
            if (claimsIdentity == null)
            {
                claimsIdentity = principal.Identities.FirstOrDefault(identity => identity.AuthenticationType == SessionCookieAuthenticationDefaults.AuthenticationScheme);
            }

            return claimsIdentity;
        }

        public static IEnumerable<Claim> IntoClaims(this SessionInfo sessionInfo)
        {
            var claims = new List<Claim> {
                new Claim(CreyClaimTypes.AccountId, sessionInfo.AccountId.ToString()),
                new Claim(CreyClaimTypes.Key, sessionInfo.Key),
                new Claim(CreyClaimTypes.IsDeleted, sessionInfo.IsDeleted.ToString())
            };

            // TODO: remove the IF statement and add the NameIdentifier claim in the initial list above once we can reasonably assume that each user cookie has been updated
            if (!string.IsNullOrEmpty(sessionInfo.UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sessionInfo.UserId));
            }

            foreach (var role in sessionInfo.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, sessionInfo.AuthenticationMethod));

            return claims;
        }
    }
}
