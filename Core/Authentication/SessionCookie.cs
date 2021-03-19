using Crey.Instrumentation.Configuration;
using Crey.Instrumentation.Web;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Crey.Authentication
{
    public static class SessionCookieAuthenticationDefaults
    {
        public const string AuthenticationScheme = "Crey.Session";
    }

    public class SessionCookieOptions
    {
        public CookieAuthenticationEvents CookieAuthenticationEvents { get; }
        public string AuthenticationType { get; }
        public string LoginPath { get; }
        public string LogoutPath { get; }

        public SessionCookieOptions(IConfiguration configuration, string serviceName)
        {
            CookieAuthenticationEvents = new SessionCookieAuthenticationEvents(serviceName);
            AuthenticationType = SessionCookieAuthenticationDefaults.AuthenticationScheme;
            LoginPath = CookieAuthenticationDefaults.LoginPath;
            LogoutPath = CookieAuthenticationDefaults.LogoutPath;
        }

        public SessionCookieOptions(
            CookieAuthenticationEvents cookieAuthenticationEvents,
            string authenticationType,
            string loginPath,
            string logoutPath)
        {
            CookieAuthenticationEvents = cookieAuthenticationEvents;
            AuthenticationType = authenticationType;
            LoginPath = loginPath;
            LogoutPath = logoutPath;
        }
    }

    public static class SessionCookieAuthenticationExtensions
    {
        public static AuthenticationBuilder AddSessionCookieAuthentication(
            this AuthenticationBuilder authenticationBuilder,
            IConfiguration configuration,
            SessionCookieOptions sessionCookieOptions)
        {
            return authenticationBuilder.AddCookie(SessionCookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = configuration.GetSessionCookieName();
                options.Cookie.Domain = configuration.GetCookieDomain();
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.ExpireTimeSpan = TimeSpan.FromDays(365); // TODO: remove when cookie refresh is implemented
                options.SlidingExpiration = false;
                options.Events = sessionCookieOptions.CookieAuthenticationEvents;
                options.TicketDataFormat = new SessionCookieDataFormat(configuration, sessionCookieOptions.AuthenticationType);
                options.LoginPath = new PathString(sessionCookieOptions.LoginPath);
                options.LogoutPath = new PathString(sessionCookieOptions.LogoutPath);
            });
        }

        public static string GetCookieValueFromUri(this CookieContainer cookieContainer, string uri, string cookieName)
        {
            var cookieCollection = cookieContainer.GetCookies(new Uri(uri)).Cast<Cookie>();
            return cookieCollection.FirstOrDefault(x => x.Name == cookieName).Value;
        }

        public static void HandleRedirect(this RedirectContext<CookieAuthenticationOptions> context, int statusCode)
        {
            if (context.Request.Path.Value.Contains("/api/")            // usual rest API
                || context.Request.Path.Value.Contains("/s2s/")         // service 2 service, custom api-key
                || context.Request.Path.Value.Contains("/cdn/")         // for cdn - no cookie auth, no user dependency
                || context.Request.Path.Value.Contains("/graphql")      // GQL
                || context.Request.Path.Value.Contains("/v1/")          // deprecated, used prior we had the other prefixes
                || context.Request.Path.Value.Contains("/adminapi/")    // deprecated don't use
                )
            {
                context.Response.StatusCode = statusCode;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
        }
    }

    public class SessionCookieDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        protected readonly string secret_;
        protected readonly string authenticationType_;

        public SessionCookieDataFormat(IConfiguration configuration, string authenticationType)
        {
            secret_ = configuration.GetValue<string>("COOKIE-SESSIONINFO-SECRET");
            authenticationType_ = authenticationType;
        }

        public string Protect(AuthenticationTicket data)
        {
            SessionInfo sessionInfo = data.Principal.IntoSessionInfo();
            return SessionInfoSigner.CreateSignedToken(sessionInfo, secret_);
        }

        public string Protect(AuthenticationTicket data, string purpose)
        {
            return Protect(data);
        }

        public virtual AuthenticationTicket Unprotect(string protectedText)
        {
            var sessionInfo = SessionInfoSigner.CreateFromSignedToken(protectedText, secret_);
            var claimsIdentity = new ClaimsIdentity(sessionInfo.IntoClaims(), authenticationType_);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            return new AuthenticationTicket(claimsPrincipal, SessionCookieAuthenticationDefaults.AuthenticationScheme);
        }

        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            return Unprotect(protectedText);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthenticateStrictAttribute : Attribute
    {
    }

    public class AuthorizeCreyAttribute : AuthorizeAttribute
    {
        public AuthorizeCreyAttribute()
        {
            Policy = CreyAuthorizationDefaults.CreyUser;
        }
    }

    public class SessionCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private string _serviceName;

        public SessionCookieAuthenticationEvents(string serviceName)
        {
            _serviceName = serviceName;
        }

        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.HandleRedirect(StatusCodes.Status401Unauthorized);
            return Task.CompletedTask;
        }

        public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.HandleRedirect(StatusCodes.Status403Forbidden);
            return Task.CompletedTask;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var services = context.HttpContext.RequestServices;
            var sessionInfo = context.Principal.IntoSessionInfo();

            var endpoint = context.HttpContext.GetEndpoint();
            var attributes = endpoint?.Metadata.GetOrderedMetadata<AuthenticateStrictAttribute>() ?? Array.Empty<AuthenticateStrictAttribute>();
            if (attributes.Any())
            {
                var validatedSessionInfo = await ValidateAndRefreshSessionInfo(sessionInfo, context.HttpContext, services, _serviceName);

                // overwrite roles in ClaimsPrincipal
                var identity = context.Principal.GetSessionIdentity();
                var claims = identity.Claims.Where(claim => claim.Type == ClaimTypes.Role).ToList();
                foreach (var claim in claims)
                {
                    identity.RemoveClaim(claim);
                }

                foreach (var role in validatedSessionInfo.Roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            var refreshedSessionInfo = context.Principal.IntoSessionInfo();
            StoreSessionInfo(refreshedSessionInfo, context.HttpContext, services);
        }

        private async Task<SessionInfo> ValidateAndRefreshSessionInfo(SessionInfo sessionInfo, HttpContext httpContext, IServiceProvider services, string serviceName)
        {
            if (!sessionInfo.IsValid)
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Session info is invalid");
            }

            if (serviceName != "iam" && sessionInfo.IsDeleted)
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "User is deleted");
            }

            string userAgent = httpContext.Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.UserAgent];
            var checkResult = await services.GetRequiredService<CreyRestClient>().ValidateKeyAsync(sessionInfo.Key, userAgent);
            if (!checkResult.IsOk)
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, $"Invalid key: {checkResult.Error.StatusCode}, {checkResult.Error.ReasonPhrase}");
            }

            var validatedSessionInfo = checkResult.Ok;
            if (!validatedSessionInfo.CheckValidity(sessionInfo))
            {
                throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, $"Session info expired: incoming: {sessionInfo.Key}, validated: {validatedSessionInfo.Key}");
            }

            return validatedSessionInfo;
        }

        private void StoreSessionInfo(SessionInfo sessionInfo, HttpContext httpContext, IServiceProvider services)
        {
            var telemetry = httpContext.Features.Get<RequestTelemetry>();
            if (telemetry != null)
            {
                if (string.IsNullOrEmpty(telemetry.Context.User.Id) || string.IsNullOrEmpty(telemetry.Context.Session.Id))
                {
                    telemetry.Context.User.Id = sessionInfo.AccountId.ToString();
                    if (!telemetry.Context.GlobalProperties.ContainsKey("Roles"))
                    {
                        telemetry.Context.GlobalProperties.Add("Roles", string.Join(",", sessionInfo.Roles));
                    }
                }
            }
        }
    }
}
