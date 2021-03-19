using Core.Extension.CreyNamePatterns;
using Crey.Configuration.ConfigurationExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Crey.Kernel
{
    // note: moved service part to core
    public class ServiceOption
    {
        public string Service { get; set; }
        public string Changeset { get; set; }
        public string Stage { get; set; }
        public string SqlCns { get; set; }
        public string StorageAccountCns { get; set; }

        public string ChangesetURLEncoded => HttpUtility.UrlEncode(Changeset);

        public ServiceOption(IConfiguration configuration, string service)
        {
            Service = service;
            SetFromConfig(configuration);
        }

        public IServiceCollection AddServiceOptionAccessor(IServiceCollection services)
        {
            Debug.Assert(!services.HasServiceConfiguration());
            services.AddSingleton(provider => this);
            return services;
        }

        private void SetFromConfig(IConfiguration configuration)
        {
            Changeset = configuration.GetValue<string>("Changeset");
            Stage = configuration.GetStage();
            SqlCns = configuration.GetValue<string>("SqlCns");
            StorageAccountCns = configuration.GetValue<string>("StorageAccountCns");
        }

        [Obsolete]
        public string GetLogTableName()
        {
            Debug.Assert(!string.IsNullOrEmpty(Stage));
            Debug.Assert(!string.IsNullOrEmpty(Service));
            string pattern = "${stage}${Service}${Name}";
            return pattern.SubstituteCreyStagePattern(Stage, Service, "log");
        }

        public string GetSqlCns()
        {
            Debug.Assert(!string.IsNullOrEmpty(Stage));
            Debug.Assert(!string.IsNullOrEmpty(Service));
            return SqlCns.SubstituteCreyStagePattern(Stage, Service, "");
        }

        public string GetSqlCnsForService(string service)
        {
            Debug.Assert(!string.IsNullOrEmpty(Stage));
            return SqlCns.SubstituteCreyStagePattern(Stage, service, "");
        }
    }

    public static class ServiceOptionsExtensions
    {
        public static bool HasServiceConfiguration(this IServiceCollection collectionBuilder)
        {
            return collectionBuilder.Any(x => x.ServiceType == typeof(ServiceOption));
        }
    }
}
