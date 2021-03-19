using Crey.Kernel.IAM;
using Crey.Web;
using IAM.Areas.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace IAM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = IAMDefaults.SERVICE_NAME;
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .CreyConfigureAppConfiguration(IAMDefaults.SERVICE_NAME, KeyVaultPolicy.All, args)
                .ConfigureCreyMicroserviceLogging()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<HardDeleteHostedService>();
                    services.AddHostedService<ReDeleteHostedService>();
                });
        }
    }
}
