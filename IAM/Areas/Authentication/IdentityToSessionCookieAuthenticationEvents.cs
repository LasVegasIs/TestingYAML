using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web.Analytics;
using IAM.Clients;
using IAM.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    //TODO: review after .NET Core 3.0 migration if it can be replaced by SignInWithClaims
    internal class IdentityToSessionCookieAuthenticationEvents : SessionCookieAuthenticationEvents
    {
        public override Task SigningIn(CookieSigningInContext context)
        {
            var services = context.HttpContext.RequestServices;
            var sessionRepository = services.GetRequiredService<ICreyService<SessionRepository>>().Value;
            var logger = services.GetService<ILogger<IdentityToSessionCookieAuthenticationEvents>>();

            if (!context.Principal.Identities.Any(identity => identity.AuthenticationType == IdentityConstants.ApplicationScheme))
            {
                throw new AccessDeniedException("Missing identity");
            }

            ClaimsIdentity claimsIdentity = context.Principal.Identities.FirstOrDefault(identity => identity.AuthenticationType == IdentityConstants.ApplicationScheme);
            string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //logger.LogCritical($"Signing In UserId {userId} with {IdentityConstants.ApplicationScheme}");
            var siteInfo = context.HttpContext.GetSiteInfo().Result;
            var credentialType = GetCredentialType(context.Principal);
            SessionInfo sessionInfo;
            if (credentialType == CredentialType.RefreshKey)
            {
                sessionInfo = sessionRepository.RefreshSessionForIdentityUser(userId, siteInfo).Result;
            }
            else
            {
                sessionInfo = sessionRepository.CreateNewSessionForIdentityUser(userId, siteInfo, credentialType).Result;
            }

            var sessionInfoStore = services.GetRequiredService<SessionInfoStore>();
            sessionInfoStore.Value = sessionInfo;

            //logger.LogCritical($"AccountId: {ok.SessionInfo.AccountId}, Key: {ok.SessionInfo.Key}");
            var creyIdentity = new ClaimsIdentity(sessionInfo.IntoClaims(), IdentityConstants.ApplicationScheme);
            creyIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            context.Principal = new ClaimsPrincipal(creyIdentity);

            var analyticsClient = services.GetRequiredService<AnalyticsClient>();
            analyticsClient.SendLoginEvent(credentialType);

            return base.SigningIn(context);
        }

        private CredentialType GetCredentialType(ClaimsPrincipal claimsPrincipal)
        {
            var authenticationMethodReferenceClaim = claimsPrincipal.FindFirst("amr");
            if (authenticationMethodReferenceClaim != null)
            {
                return authenticationMethodReferenceClaim.Value.ToCredentialType();
            }

            var authenticationMethodClaim = claimsPrincipal.FindFirst(ClaimTypes.AuthenticationMethod);
            if (authenticationMethodClaim != null)
            {
                return authenticationMethodClaim.Value.ToCredentialType();
            }

            throw new AccessDeniedException("Unknown credential type");
        }
    }
}