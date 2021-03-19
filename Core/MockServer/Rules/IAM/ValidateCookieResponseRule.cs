using Crey.Authentication;
using Crey.Instrumentation.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
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
    public class ValidateCookieResponseRule : Rule
    {
        public int Id { get; set; }
        public IEnumerable<HttpMethod> Methods => new[] { HttpMethod.Get };
        public Regex RoutePattern => new Regex("/mock/session/validate");
        public bool IsInternal => true;
        public int Priority => -100;


        public async Task Handle(IServiceProvider serviceProvider, HttpListenerContext ctx)
        {
            var token = ctx.Request.Cookies[CreateSessionResponseRule.CookieName]?.Value ?? "";
            var tokens = HttpUtility.UrlDecode(token).Split(";", 2);

            if (tokens.Length == 2)
            {
                try
                {
                    var info = JsonConvert.DeserializeObject<SessionInfo>(tokens[1]);
                    ctx.Response.StatusCode = 200;
                    ctx.Response.AddHeader(HeaderNames.ContentType, "application/json");
                    using (var sw = new StreamWriter(ctx.Response.OutputStream))
                    {
                        // send back what we get
                        await sw.WriteAsync(tokens[1]);
                    }
                }
                catch (Exception)
                {
                    ctx.Response.StatusCode = 401;
                }
            }
            else
            {
                ctx.Response.StatusCode = 500;
                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    await sw.WriteAsync("Invalid token:[{token}]");
                }
            }
            ctx.Response.OutputStream.Close();
        }
    }
}
