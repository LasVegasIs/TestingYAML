using Core.Cache;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Kernel.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class HardDeleteHostedService : SafeHostedService
    {
        private readonly IServiceScopeFactory serviceScopeFactory_;
        private readonly IConfiguration configuration_;

        public HardDeleteHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<HardDeleteHostedService> logger)
            : base(logger)
        {
            serviceScopeFactory_ = serviceScopeFactory;
            configuration_ = configuration;
        }

        protected override TimeSpan Period => configuration_.IsProductionSlot() ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(5);

        protected override async Task Run()
        {
            using (var scope = serviceScopeFactory_.CreateScope())
            {
                var accountRepository = scope.ServiceProvider.GetRequiredService<ICreyService<AccountRepository>>();
                await accountRepository.Value.HardDeleteAccountsAsync();
            }
        }
    }
}