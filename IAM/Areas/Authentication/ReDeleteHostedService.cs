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
    public class ReDeleteHostedService : SafeHostedService
    {
        private readonly IServiceScopeFactory serviceScopeFactory_;
        private readonly IConfiguration configuraiton_;

        public ReDeleteHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuraiton,
            ILogger<ReDeleteHostedService> logger)
            : base(logger)
        {
            serviceScopeFactory_ = serviceScopeFactory;
            configuraiton_ = configuraiton;
        }

        protected override TimeSpan Period => configuraiton_.IsProductionSlot() ? TimeSpan.FromDays(1) : TimeSpan.FromMinutes(5);

        protected override async Task Run()
        {
            using (var scope = serviceScopeFactory_.CreateScope())
            {
                var accountRepository = scope.ServiceProvider.GetRequiredService<ICreyService<AccountRepository>>();
                await accountRepository.Value.ReDeleteSoftDeletedAccountsAsync();
                await accountRepository.Value.ReDeleteHardDeletedAccountsAsync();
            }
        }
    }
}