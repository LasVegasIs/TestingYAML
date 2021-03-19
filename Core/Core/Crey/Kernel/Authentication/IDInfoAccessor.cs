using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crey.Kernel.Authentication
{
    public class IDInfoAccessor : IIDInfoAccessor
    {
        private readonly IHttpContextAccessor httpContext_;
        private readonly SessionInfoStore sessionInfoStore_;

        public IDInfoAccessor(IHttpContextAccessor httpContext, SessionInfoStore sessionStore)
        {
            httpContext_ = httpContext;
            sessionInfoStore_ = sessionStore;
        }

        public SessionInfo GetSessionInfo()
        {
            SetSessionInfo();
            return sessionInfoStore_.Value;
        }

        private void SetSessionInfo()
        {
            if (sessionInfoStore_.Value == null)
            {
                // try to extract session info from claims
                var context = httpContext_.HttpContext;
                if (context != null && context.User != null)
                {
                    sessionInfoStore_.Value = context.User.IntoSessionInfo();
                }
                else
                {
                    sessionInfoStore_.Value = new SessionInfo();
                }
            }
        }
    }

    public static class HttpIDInfoExtensions
    {
        public static IServiceCollection AddIDInfoAccessor(this IServiceCollection collectionBuilder)
        {
            collectionBuilder.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            collectionBuilder.TryAddScoped<SessionInfoStore>();
            collectionBuilder.TryAddScoped<IDInfoAccessor>();
            collectionBuilder.TryAddScoped<IIDInfoAccessor>(x => x.GetService<IDInfoAccessor>());
            return collectionBuilder;
        }
    }
}
