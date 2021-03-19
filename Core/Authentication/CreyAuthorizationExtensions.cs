using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Crey.Authentication
{
    public static class CreyAuthorizationDefaults
    {
        public const string AlwaysFail = "AlwaysFail";
        public const string CreyUser = "CreyUser";
        public static readonly string[] DefaultAuthenticationSchemes = new string[] {
            SessionCookieAuthenticationDefaults.AuthenticationScheme
        };
    }

    public static class CreyAuthorizationExtensions
    {
        public static AuthorizationPolicy DefaultAuthenticationPolicy(string[] additionalSchemes)
        {
            var schemes = CreyAuthorizationDefaults.DefaultAuthenticationSchemes.Concat(additionalSchemes).ToArray();

            return new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(schemes)
                .RequireAssertion(_ => true)
                .Build();
        }

        public static AuthorizationOptions AddCreyAuthorization(this AuthorizationOptions authorizationOptions, IConfiguration configuration, string[] additionalSchemes)
        {
            var schemes = CreyAuthorizationDefaults.DefaultAuthenticationSchemes.Concat(additionalSchemes).ToArray();

            authorizationOptions.AddPolicy(CreyAuthorizationDefaults.CreyUser, policyBuilder =>
            {
                policyBuilder
                    .AddAuthenticationSchemes(schemes)
                    .RequireAuthenticatedUser();
            });

            authorizationOptions.AddPolicy(CreyAuthorizationDefaults.AlwaysFail, policyBuilder =>
            {
                policyBuilder
                    .AddAuthenticationSchemes(schemes)
                    .RequireAssertion(x => false);

            });

            return authorizationOptions;
        }
    }
}
