using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockServer.Rules;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MockServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine($"Usage: MockServer service vault [options]");
                return;
            }

            var service = args[0];
            var vault = Enum.Parse<KeyVaultPolicy>(args[1]);
            var di = new DIContext(args, service, vault);

            HandleRequests(di.ServiceProvider.GetRequiredService<IServiceScopeFactory>()).Wait();
        }

        static async Task HandleRequests(IServiceScopeFactory serviceScopeFactory)
        {
            const int PORT = 9100;

            var rules = new RuleHandler();
            rules.AddDefaults();

            var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{PORT}/");
            Console.WriteLine($"Listening on port {PORT}");
            listener.Start();

            while (true)
            {
                var ctx = await listener.GetContextAsync();
                // by default it is 404
                ctx.Response.StatusCode = 404; 
                try
                {
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        await rules.HandleRequest(scope.ServiceProvider, ctx);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ctx.Response.StatusCode = 500;
                }

                // make sure we send it !
                ctx.Response.OutputStream.Close();
            }
        }
    }
}
