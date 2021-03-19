using Microsoft.Extensions.Configuration;

namespace Crey.Data.Azure
{
    public static class ConfigurationExtensions
    {
        public static string GetStorageAccountConnectionString(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("StorageAccountCns");
        }
    }
}