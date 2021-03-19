using Crey.Kernel.ServiceDiscovery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Kernel.Authentication
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddCreyClientAuthenticationAndAuthorization(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration,
            SessionCookieOptions sessionCookieOptions = null,
            params string[] additionalSchemes)
        {
            var authenticationBuilder = collectionBuilder
                .AddSingleton<IAuthorizationHandler, RoleAuthorizationRequirementHandler>()
                .AddAuthorization(options =>
                {
                    options.AddCreyAuthorization(configuration, additionalSchemes);
                    options.DefaultPolicy = options.GetPolicy(CreyAuthorizationDefaults.CreyUser);
                })
                .AddAuthentication()
                .AddSessionCookieAuthentication(configuration, (sessionCookieOptions == null)
                    ? new SessionCookieOptions(configuration)
                    : sessionCookieOptions);

            return authenticationBuilder;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection collectionBuilder)
        {
            collectionBuilder.AddCreyRestClientFactory();
            return collectionBuilder;
        }
    }
}
