using Crey.Configuration.ConfigurationExtensions;
using Crey.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IAM.Areas.Identity.Extensions
{
    public static class PageModelExtensions
    {
        public static string TranslateModelErrorCode(this string error)
        {
            if (string.IsNullOrEmpty(error))
                return string.Empty;
            if (error == "DuplicateUserName")
                return "Input.UserName";
            if (error == "DuplicateEmail")
                return "Input.Email";
            if (error.StartsWith("Input."))
                return error;
            return string.Empty;
        }

        public static IActionResult WhitelistedRedirect(this PageModel pageModel, string redirectUrl, string mobileMode = null)
        {
            redirectUrl = redirectUrl ?? pageModel.Url.Content("~/");

            if (pageModel.Url.IsLocalUrl(redirectUrl))
            {
                if (mobileMode == null)
                {
                    return pageModel.LocalRedirect(redirectUrl);
                }
                else
                {
                    var parameters = new Dictionary<string, string> { { "mobile", mobileMode } };
                    var newRedirectUrl = QueryHelpers.AddQueryString(redirectUrl, parameters);
                    return pageModel.LocalRedirect(newRedirectUrl);
                }
            }

            var configuration = pageModel.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var allowedReferrers = configuration.GetAllowedReferrers();

            var uri = new Uri(redirectUrl);
            string uriString = $"{uri.Scheme}://{uri.Host}";
            if (uri.Port != 443)
            {
                uriString = uriString + $":{uri.Port}";
            }

            if (allowedReferrers.Contains(uriString))
            {
                return new RedirectResult(redirectUrl);
            }

            throw new InvalidArgumentException($"{redirectUrl} is not an allowed redirect URL.");
        }

        public static void UpdateTheme(this PageModel pageModel, string theme = null)
        {
            if (theme != null)
            {
                pageModel.HttpContext.Session.SetString("theme", theme);
            }
        }
    }
}