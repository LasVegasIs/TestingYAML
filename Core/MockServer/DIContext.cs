using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockServer.Rules.IAM;
using System;

namespace MockServer
{
#nullable enable
    public class DIContext
    {
        IServiceCollection _serviceCollection;

        public IServiceProvider ServiceProvider { get; private set; }

        public DIContext(string[] args, string service, KeyVaultPolicy vaultPolicy)
        {
            _serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddCreyConfigurations(args, service, "Development", vaultPolicy)
                .Build();

            _serviceCollection.AddSingleton<IConfiguration>(config);
            _serviceCollection.AddSingleton<SessionStore>();

            ServiceProvider = _serviceCollection.BuildServiceProvider();
        }
    }
}
