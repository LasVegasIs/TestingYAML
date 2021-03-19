using Core.Functional;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Contracts.Matchmaking;
using Crey.Kernel.Authentication;
using Crey.Kernel.Avatar;
using Crey.Kernel.Comments;
using Crey.Kernel.GameResources;
using Crey.Kernel.Levels;
using Crey.Kernel.Notification;
using Crey.Kernel.Prefabs;
using Crey.Kernel.Proxy;
using Microsoft.Extensions.Configuration;

namespace Crey.Kernel.ServiceDiscovery
{
    public class ProvidedService : IProvidedService
    {
        private readonly IConfiguration configuration_;
        private readonly ServiceOption serviceOption_;

        public ProvidedService(IConfiguration configuration, ServiceOption serviceOption)
        {
            configuration_ = configuration;
            serviceOption_ = serviceOption;
        }

        public Result<ServiceInfo, Error> GetInfo()
        {
            var info = new ServiceInfo()
            {
                Name = serviceOption_.Service,
                Changeset = serviceOption_.Changeset,
                Stage = serviceOption_.Stage,
                DeploymentSlot = configuration_.GetDeploymentSlot().ToString()
            };
            return info;
        }

        public Result<string, Error> GetServiceURI(string service)
        {
            return configuration_.GetBaseURI(service);
        }

        public Result<ServiceURIResult, Error> GetServiceURIs()
        {
            return new ServiceURIResult
            {
                AuthServiceAddress = configuration_.GetBaseURI(AuthenticationDefaults.SERVICE_NAME),
                CommentsServiceAddress = configuration_.GetBaseURI(CommentsDefaults.SERVICE_NAME),
                LevelsServiceAddress = configuration_.GetBaseURI(LevelsDefaults.SERVICE_NAME),
                NotificationServiceAddress = configuration_.GetBaseURI(NotificationDefaults.SERVICE_NAME),
                PrefabsServiceAddress = configuration_.GetBaseURI(PrefabsDefaults.SERVICE_NAME),
                GameResourcesServiceAddress = configuration_.GetBaseURI(GameResourcesDefaults.SERVICE_NAME),
                MatchmakingServiceAddress = configuration_.GetBaseURI(MatchmakingDefaults.SERVICE_NAME),
                AvatarServiceAddress = configuration_.GetBaseURI(AvatarDefaults.SERVICE_NAME),
                WebsiteAddress = configuration_.GetBaseURI(ProxyDefaults.SERVICE_NAME),
                LauncherDataAddress = "https://creylauncher.blob.core.windows.net/${slot}-launcher",
            };
        }
    }
}
