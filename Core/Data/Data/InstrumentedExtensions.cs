#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace Crey.Data
{
    public static class InstrumentedExtensions
    {
        public static IServiceCollection AddInstrumentedDbContext<TContext>(this IServiceCollection serviceCollection,
          Action<DbContextOptionsBuilder>? optionsAction = null,
          ServiceLifetime contextLifetime = ServiceLifetime.Scoped, ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            serviceCollection.AddDbContext<TContext>(optionsAction, contextLifetime, optionsLifetime);
            // slq table metrics are in sql management, buf if needed can add prometheus here
            serviceCollection.AddHealthChecks()
                .AddDbContextCheck<TContext>(tags: new[] { "storages" });

            return serviceCollection;
        }
    }
}
