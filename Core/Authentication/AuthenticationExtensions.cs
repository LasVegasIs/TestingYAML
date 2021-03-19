using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Authentication
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddCreyClientAuthenticationAndAuthorization(
            this IServiceCollection collectionBuilder,
            IConfiguration configuration,
            string serviceName,
            SessionCookieOptions sessionCookieOptions = null,
            params string[] additionalSchemes)
        {
            var authenticationBuilder = collectionBuilder
                .AddAuthorization(options =>
                {
                    options.AddCreyAuthorization(configuration, additionalSchemes);
                    options.DefaultPolicy = options.GetPolicy(CreyAuthorizationDefaults.CreyUser);
                })
                .AddAuthentication()
                .AddSessionCookieAuthentication(configuration, (sessionCookieOptions == null)
                    ? new SessionCookieOptions(configuration, serviceName)
                    : sessionCookieOptions);

            return authenticationBuilder;
        }

        public static MvcOptions AddBasicChecks(this MvcOptions options)
        {
            options.Filters.Add(new AuthorizeFilter(CreyAuthorizationExtensions.DefaultAuthenticationPolicy(new string[] { })));
            return options;
        }
    }
}
