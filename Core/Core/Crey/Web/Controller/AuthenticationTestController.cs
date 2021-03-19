#nullable enable
using Crey.Contracts;
using Crey.FeatureControl;
using Crey.Kernel.Authentication;
using Crey.Web.Service2Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Crey.FeatureControl.GeoLocationService;

namespace Crey.Web.Controllers
{
    public class AuthorizationTestController : Controller
    {
        private readonly IIDInfoAccessor idInfo_;
        private readonly ILogger logger_;

        public AuthorizationTestController(IIDInfoAccessor idInfo,
            ILogger<AuthorizationTestController> logger)
        {
            idInfo_ = idInfo;
            logger_ = logger;
        }

        [HttpGet("/api/v1/test/log")]
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

        [HttpGet("/api/v1/test/authorize")]
        [Authorize]
        public ActionResult TestAuthorize()
        {
            return Ok();
        }

        [HttpGet("/api/v1/test/authorize/role")]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.ContentDev)]
        public ActionResult TestAuthorizeWithRole()
        {
            return Ok();
        }

        [HttpGet("/api/v1/test/authorizestrict")]
        [AuthenticateStrict]
        [Authorize]
        public ActionResult TestAuthorizeStrict()
        {
            return Ok();
        }

        [HttpGet("/api/v1/test/authorizestrict/role")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.ContentDev)]
        public ActionResult TestAuthorizeStrictWithRole()
        {
            return Ok();
        }

        [HttpGet("/api/v1/test/whoami/auth")]
        [Authorize]
        public ActionResult<int> WhoAmI1()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v1/test/whoami/noauth")]
        public ActionResult<int> WhoAmI2()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v1/test/authorizeinternal")]
        [Authorize]
        [ServerToServer]
        public ActionResult TestAuthorizeInternal()
        {
            return Ok();
        }

        [HttpGet("/api/v1/test/server2server/oldkeyonly")]
        [Authorize]
        [ServerToServer]
        public ActionResult<int> TestS2SOld()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v1/test/server2server/oldwithuser")]
        [Authorize]
        [ServerToServer]
        public ActionResult<int> TestS2SOldWithUser()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v1/test/server2server/noauth")]
        [ServerToServer]
        public ActionResult<int> TestS2S()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        [HttpGet("/api/v1/test/server2server/auth")]
        [ServerToServer]
        [Authorize]
        public ActionResult<int> TestS2SA()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return sessionInfo.AccountId;
        }

        public class WhereIAm
        {
            public IpGeo? Geo { get; set; }
            public string? RemoteIpAddress { get; set; }
            public StringValues XForwardedFor { get; set; }
        }

        [HttpGet("/api/v1/test/whereiam/ip")]
        [Authorize]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
        public async Task<WhereIAm> GetWhereIAm([FromServices] GeoLocationService geoLocation, string? any = null)
        {
            //TODO: what about
            // consider nullables being as Options, so that any nullable can be mapped
            // MapOrElse<T?, U>(this T? self, F<T,U> f, D<U> d);
            // var geo = await any.MapOrElseAsync(x=> geoLocation.GetLocation(IPAddress.Parse(any!)), () => geoLocation.GetLocation(HttpContext));
            // also it does not shorter, but allows to fluent compose with other fluent ops like in rust (and has same API as Resul<T,E> we have)
            var geo = any != null ? await geoLocation.GetLocation(IPAddress.Parse(any!)) : await geoLocation.GetLocation(HttpContext);
            var remoteIpAddress = base.HttpContext.Connection.RemoteIpAddress?.ToString();
            HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var xForwardedFor);
            return new WhereIAm
            {
                Geo = geo,
                RemoteIpAddress = remoteIpAddress,
                XForwardedFor = xForwardedFor,
            };
        }
    }
}
