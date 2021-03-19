using Crey.Contracts;
using Crey.Exceptions;
using Crey.Misc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Crey.Kernel.Authentication
{
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
                throw new AccessDeniedException("Malformed signedToken");
            }

            string cookieSignature = cookieComponents[0];
            string cookieData = cookieComponents[1];

            if (cookieSignature == null || cookieSignature == "")
            {
                throw new AccessDeniedException("Missing signature");
            }

            var signature = CreateSignature(cookieData, secret);
            if (signature != cookieSignature)
            {
                throw new AccessDeniedException("Signature mismatch");
            }

            var sessionInfo = JsonConvert.DeserializeObject<SessionInfo>(cookieData);
            if (sessionInfo == null)
            {
                throw new AccessDeniedException("Malformed signedToken");
            }

            return sessionInfo;
        }

        private static string CreateSignature(string data, string secret)
        {
            Trace.Assert(!string.IsNullOrEmpty(data));
            return (secret == "") ? "" : CryptoHelper.CreyCryptoHash(data + secret);
        }
    }

    public class SessionInfoStore
    {
        private readonly string secret_;

        public SessionInfo Value { get; set; }

        public SessionInfoStore(IConfiguration configuration)
        {
            secret_ = configuration.GetValue<string>("COOKIE-SESSIONINFO-SECRET");
        }

        public string GetSignedTokenString()
        {
            if (Value == null)
            {
                return null;
            }

            return SessionInfoSigner.CreateSignedToken(Value, secret_);
        }

        public void SetFromSignedToken(string signedToken)
        {
            Value = SessionInfoSigner.CreateFromSignedToken(signedToken, secret_);
        }
    }

    public static class SessionInfoExtensions
    {
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

        public static string IntoIDToken(this SessionInfo sessionInfo, string secretKey)
        {
            var claims = sessionInfo.IntoClaims();

            //todo: generate a salt and add it to the jwt
            //   secretkey := secretKey + salt
            //   idtoken = salt + jwt

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "playcrey.com",
                audience: "playcrey.com",
                claims: claims,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
