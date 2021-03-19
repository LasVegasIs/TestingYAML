using Crey.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MockServer.Rules.IAM
{
    public class CreateSessionResponseRule : Rule
    {
        public const string CookieName = "Crey.Dev.Session";

        public int Id { get; set; }
        public IEnumerable<HttpMethod> Methods => new[] { HttpMethod.Get };
        public Regex RoutePattern => new Regex("/mock/session/create");
        public bool IsInternal => true;
        public int Priority => -100;


        public async Task Handle(IServiceProvider serviceProvider, HttpListenerContext ctx)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var sessionStore = serviceProvider.GetRequiredService<SessionStore>();

            var accountId = int.Parse(ctx.Request.QueryString["accountid"] ?? "0");
            var roles = ctx.Request.QueryString["roles"] ?? "";
            var setCookie = bool.Parse(ctx.Request.QueryString["setcookie"] ?? "false");

            var guid = Guid.NewGuid();
            var sessionInfo = new SessionInfo
            {
                AccountId = accountId,
                Roles = roles.Split(',').Select(x => x.Trim()).ToHashSet(),
                UserId = guid.ToString(),
                Key = $"test-{guid}",
                AuthenticationMethod = "Test",
            };

            sessionStore.Add(sessionInfo);

            var secret = configuration.GetValue<string>("COOKIE-SESSIONINFO-SECRET");
            var token = SessionInfoSigner.CreateSignedToken(sessionInfo, secret);

            /// compose response
            ctx.Response.StatusCode = 200;
            if (setCookie)
            {
                var tokenUrlSafe = HttpUtility.UrlEncode(token);
                var cookie = new Cookie(CookieName, tokenUrlSafe);
                cookie.HttpOnly = false;
                cookie.Secure = false;
                cookie.Domain = "playcrey.com";
                cookie.Path = "/";
                cookie.Expires = DateTime.UtcNow + TimeSpan.FromDays(1);
                ctx.Response.Cookies.Add(cookie);
            }
            using (var sw = new StreamWriter(ctx.Response.OutputStream))
            {
                await sw.WriteAsync(token);
            }
            ctx.Response.OutputStream.Close();
        }
    }
}
