using Crey.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Crey.Kernel.Authentication
{
    public static class CreyMvcFilterExtensions
    {
        public static MvcOptions AddBasicChecks(this MvcOptions options)
        {
            options.Filters.Add(new AuthorizeFilter(CreyAuthorizationExtensions.DefaultAuthenticationPolicy(new string[] { })));
            options.Filters.Add(new ModelStateCheckFilter());
            return options;
        }

        public static MvcOptions AddBasicChecksAuthHack(this MvcOptions options, string[] additionalSchemes)
        {
            options.Filters.Add(new AuthorizeFilter(CreyAuthorizationExtensions.DefaultAuthenticationPolicy(additionalSchemes)));
            options.Filters.Add(new ModelStateCheckFilter());
            return options;
        }
    }
}
