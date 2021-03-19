using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Crey.Authentication
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthorizationTestController : Controller
    {
        private readonly ILogger logger_;

        public AuthorizationTestController(ILogger<AuthorizationTestController> logger)
        {
            logger_ = logger;
        }

        [HttpGet("/api/v2/test/log")]
        public async Task<ActionResult<string>> LogRequest()
        {
            var builder = new StringBuilder();
            try
            {
                builder.AppendLine($"Request query: {Request.QueryString}");
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    builder.AppendLine($"Request body: {body}");
                }
            }
            catch (Exception ex)
            {
                builder.AppendLine($"Request query,bpdy parse failed: {ex}");
            }

            foreach (var h in HttpContext.Request.Headers)
            {
                if (h.Key != HeaderNames.Cookie)
                {
                    builder.AppendLine($"Request headers: [{h.Key}] = [{h.Value}]");
                }
            }

            foreach (var c in HttpContext.Request.Cookies)
            {
                builder.AppendLine($"Request cookie: [{c.Key}] = [{c.Value}]");
            }

            var str = builder.ToString();
            logger_.LogCritical(str);
            return str;
        }

        [HttpGet("/api/v2/test/authorize")]
        [Authorize]
        public ActionResult TestAuthorize()
        {
            return Ok();
        }

        [HttpGet("/api/v2/test/authorize/role")]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
        public ActionResult TestAuthorizeWithRole()
        {
            return Ok();
        }

        [HttpGet("/api/v2/test/authorizestrict")]
        [AuthenticateStrict]
        [Authorize]
        public ActionResult TestAuthorizeStrict()
        {
            return Ok();
        }

        [HttpGet("/api/v2/test/authorizestrict/role")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
        public ActionResult TestAuthorizeStrictWithRole()
        {
            return Ok();
        }

        [HttpGet("/api/v2/test/whoami/auth")]
        [Authorize]
        public ActionResult<int> WhoAmI1()
        {
            var sessionInfo = HttpContext.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v2/test/whoami/noauth")]
        public ActionResult<int> WhoAmI2()
        {
            var sessionInfo = HttpContext.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v2/test/server2server/noauth")]
        [ServerToServer]
        public ActionResult<int> TestS2S()
        {
            var sessionInfo = HttpContext.GetSessionInfo();
            return sessionInfo.AccountId;
        }
    }
}
