using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crey.Authentication
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
