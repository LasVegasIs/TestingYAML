using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockServer.Rules
{
    class RuleHandler
    {
        readonly List<Rule> _rules = new List<Rule>();
        int _idCounter = 0;

        public void AddRule(Rule rule)
        {
            _idCounter += 1;
            rule.Id = _idCounter;
            _rules.Add(rule);
        }

        public void RemoveRule(int id)
        {
            _rules.RemoveAll(x => x.Id == id && !x.IsInternal);
        }

        public void AddDefaults()
        {
            AddRule(new IAM.CreateSessionResponseRule());
            AddRule(new IAM.ValidateCookieResponseRule());
            AddRule(new IAM.ValidateKeyResponseRule());
        }

        public async Task HandleRequest(IServiceProvider serviceProvider, HttpListenerContext ctx)
        {
            var path = ctx.Request.Url?.AbsolutePath ?? "";
            if (string.IsNullOrEmpty(path))
                return;

            switch (path)
            {
                case "/mock/rules": await ManageRoles(ctx); break;

                default: await HandleRule(serviceProvider, ctx, path); break;
            }
        }

        private async Task HandleRule(IServiceProvider serviceProvider, HttpListenerContext ctx, string path)
        {
            var rules = _rules
                                .Where(x => x.RoutePattern.IsMatch(path))
                                .OrderBy(x => x.Priority);
            if(!rules.Any())
            {
                Console.WriteLine($"No rule found for {path}");
                ctx.Response.StatusCode = 404;
                ctx.Response.OutputStream.Close();
                return;
            }

            Console.WriteLine($"Apply rules for {path}");
            foreach (var rule in rules)
            {
                Console.WriteLine($"  {rule.RoutePattern}");
                await rule.Handle(serviceProvider, ctx);
            }
        }

        private async Task ManageRoles(HttpListenerContext ctx)
        {
            var method = new HttpMethod(ctx.Request.HttpMethod);
            if (method == HttpMethod.Get)
            {
                ctx.Response.StatusCode = 200;
                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    var rules = _rules.OrderBy(x => x.Priority).ToList();
                    await sw.WriteAsync(JsonConvert.SerializeObject(rules));
                }
                ctx.Response.OutputStream.Close();
            }
            else if (method == HttpMethod.Post)
            {
                //todo. add rule
            }
            else if (method == HttpMethod.Delete)
            {
                var id = int.Parse(ctx.Request.QueryString["id"] ?? "");
                RemoveRule(id);
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                ctx.Response.OutputStream.Close();
            }
        }
    }
}
