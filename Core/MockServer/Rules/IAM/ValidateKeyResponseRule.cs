using Crey.Instrumentation.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MockServer.Rules.IAM
{
    public class ValidateKeyResponseRule : Rule
    {
        public int Id { get; set; }
        public IEnumerable<HttpMethod> Methods => new[] { HttpMethod.Post };
        public Regex RoutePattern => new Regex("/iam/s2s/accounts/validate/key");
        public bool IsInternal => true;
        public int Priority => -100;


        private class Input
        {
            public string? Key { get; set; }
        }

        public async Task Handle(IServiceProvider serviceProvider, HttpListenerContext ctx)
        {
            var sessionStore = serviceProvider.GetRequiredService<SessionStore>();

            string body = await new StreamReader(ctx.Request.InputStream).ReadToEndAsync();
            var key = JsonConvert.DeserializeObject<Input>(body);
            if (key.Key == null)
                throw new InvalidArgumentException($"Missing key");

            var info = sessionStore.Get(key.Key);
            if (info != null)
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.AddHeader(HeaderNames.ContentType, "application/json");
                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    // send back what we get
                    var json = JsonConvert.SerializeObject(info);
                    await sw.WriteAsync(json);
                }
            }
            else
            {
                ctx.Response.StatusCode = 401;
            }
            ctx.Response.OutputStream.Close();
        }
    }
}
