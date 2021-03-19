using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Crey.MigrationTool
{
    public static class OnDemandSqlMigrationExtension
    {
        public static IApplicationBuilder MigrateSqlDatabase<DB>(this IApplicationBuilder app)
            where DB : DbContext
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DB>())
                {
                    context.Database.Migrate();
                }
            }

            return app;
        }
    }
}
