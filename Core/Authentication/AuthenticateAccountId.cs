using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Crey.Authentication
{
    public static class AccountIdAuthenticationDefaults
    {
        public const string AuthenticationScheme = "AccountId";
    }

    public class AccountIdAuthenticationOptions : AuthenticationSchemeOptions
    {
        public Func<HttpRequest, Task<int>> ExtractAccountId { get; set; }
    }

    public class AccountIdAuthenticationHandler : AuthenticationHandler<AccountIdAuthenticationOptions>
    {
        public readonly CreyRestClient creyRestClient_;

        public AccountIdAuthenticationHandler(
            IOptionsMonitor<AccountIdAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            CreyRestClient creyRestClient)
            : base(options, logger, encoder, clock)
        {
            creyRestClient_ = creyRestClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            IEnumerable<Claim> claims;
            try
            {
                int id = await Options.ExtractAccountId(Request);
                var sessionInfo = await creyRestClient_.ImpersonateAccount(id);
                claims = sessionInfo.IntoClaims();
            }
            catch (Exception ex)
            {
                Logger.LogCritical("Failed to get session info: {}", ex);
                return AuthenticateResult.Fail("Missing account id");
            }

            var claimIdentity = new ClaimsIdentity(claims, AccountIdAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimIdentity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, AccountIdAuthenticationDefaults.AuthenticationScheme));
        }
    }

    public static class AccountIdAuthenticationExtensions
    {
        public static AuthenticationBuilder AddAccountIdAuthentication(this AuthenticationBuilder authenticationBuilder, string schemaName, Action<AccountIdAuthenticationOptions> configureOptions)
        {
            return authenticationBuilder.AddScheme<AccountIdAuthenticationOptions, AccountIdAuthenticationHandler>(schemaName, configureOptions);
        }
    }
}
