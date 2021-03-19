using Crey.Kernel;
using Crey.Kernel.Authentication;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    // note: moved to net50
    // other improvements - add service EP via k8s discovery (use internal DNS of k8s)
    public static class InstrumentedExtensions
    {
        public static IServiceCollection AddInstrumentedDbContext<TContext>(this IServiceCollection serviceCollection,
          ServiceOption serviceOption, Action<DbContextOptionsBuilder> optionsAction = null,
          ServiceLifetime contextLifetime = ServiceLifetime.Scoped, ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            serviceCollection.AddDbContext<TContext>(optionsAction, contextLifetime, optionsLifetime);
            // slq table merics are in sql management, buf if needed can add promql here
            serviceCollection.AddHealthChecks()
                .AddDbContextCheck<TContext>(tags: new[] { "storages" });

            return serviceCollection;
        }
    }
}
