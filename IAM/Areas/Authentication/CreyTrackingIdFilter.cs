using System.Threading.Tasks;
using Crey.Misc;
using Crey.Web.Analytics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace IAM.Areas.Authentication
{
    public class CreyTrackingIdFilter : IAsyncPageFilter
    {
        private readonly IConfiguration configuration_;

        public CreyTrackingIdFilter(IConfiguration configuration)
        {
            configuration_ = configuration;
        }

        public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var trackingIdCookieName = configuration_.GetTrackingIdCookieName();
            if (!context.HttpContext.Request.Cookies.ContainsKey(trackingIdCookieName))
            {
                context.HttpContext.Response.Cookies.Append(trackingIdCookieName, CryptoHelper.CreateKey(10));  // limit for client
            }

            return next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}