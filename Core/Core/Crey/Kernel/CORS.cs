using Crey.Configuration.ConfigurationExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Kernel
{
    public static class CORSExtensions
    {
        public static void EnableCreyCors(this IServiceCollection collectionBuilder, IConfiguration configuration)
        {
            collectionBuilder.AddCors(options =>
            {
                options.AddDefaultPolicy(
                   builder =>
                   {
                       builder
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithOrigins(configuration.GetAllowedReferrers());
                   }
                );
            });
        }
    }
}
