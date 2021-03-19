#nullable enable
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crey.Data.Sql
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder MigrateApplicationDB<T>(this IApplicationBuilder app)
            where T : DbContext
        {
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetService<T>())
                    {
                        context!.Database.Migrate();
                    }
                }
            }
            catch (Exception ex)
            {
                var log = app.ApplicationServices.GetService<ILogger>();
                log?.LogCritical(ex, "Failed to initialize DB");
                throw;
            }

            return app;
        }
    }
}