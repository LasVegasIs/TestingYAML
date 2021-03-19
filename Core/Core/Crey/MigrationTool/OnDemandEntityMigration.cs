using Core.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Crey.MigrationTool
{
    public interface IMigrationId
    {
        string Id { get; }
    }

    public class OnDemandEntityMigration<T>
        where T : class, IMigrationId
    {
        private readonly ILogger logger_;
        private readonly MigrationTableStorage migrationTable_;

        public OnDemandEntityMigration(AzureContext context, ILogger logger)
        {
            logger_ = logger;
            migrationTable_ = context.CreateMigrationStorage<T>(null);
        }

        public async Task MarkMigrated(T data)
        {
            await Migrate(data, (x) => true, (x) => true, (x) => { });
        }

        public async Task Migrate(T data, Func<T, bool> checkPrelock, Func<T, bool> checkInlock, Action<T> migrate)
        {
            var id = data.Id;

            if (!checkPrelock(data))
            {
                return;
            }

            var migrateToken = await migrationTable_.StartMigration(logger_, id);
            if (migrateToken.Status == MigrationStatus.Migrating)
            {
                try
                {
                    if (checkInlock(data))
                        migrate(data);
                }
                catch (Exception ex)
                {
                    logger_?.LogCritical($"Migration failed: {ex}");
                    await migrationTable_.MigrationFailed(logger_, migrateToken, ex.Message);
                    throw ex;
                }
                await migrationTable_.MigrationDone(logger_, migrateToken);
            }
        }
    }

    public static class OnDemandEntityMigrationExtension
    {
        public static IServiceCollection AddOnDemandEntityMigrate<T>(this IServiceCollection services)
            where T : class, IMigrationId
        {
            services.AddSingleton<OnDemandEntityMigration<T>>();
            return services;
        }
    }
}
