using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Crey.Kernel.Levels
{
    public static class LevelsExtensions
    {
        public static IServiceCollection AddLevelsService(this IServiceCollection collectionBuilder)
        {
            Debug.Assert(collectionBuilder.HasIDInfoAccessor());

            collectionBuilder
                .AddCreyRestClientFactory()
                .AddScopedCreyService<IThumbnailVoteRepository, RestThumbnailVoteRepository>()
                /*.AddScopedCreyService<IBadgesRepository, RestBadgesRepository>()
                .AddScopedCreyService<ILeaderboardRepository, RestLeaderboardRepository>()*/;
            return collectionBuilder;
        }
    }
}
