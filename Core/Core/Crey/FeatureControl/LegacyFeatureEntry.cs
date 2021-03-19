using Crey.Contracts;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    [Obsolete]
    public class LegacyFeatureEntry : TableEntity
    {
        [IgnoreProperty]
        public string Name
        {
            get => RowKey;
            set => RowKey = value;
        }

        public bool Disabled { get; set; }

        [IgnoreProperty]
        public List<string> RequiredRoles { get; set; }
        public string AZRequiredRoles
        {
            get => RequiredRoles == null || !RequiredRoles.Any() ? null : string.Join(",", RequiredRoles);
            set => RequiredRoles = value == null ? null : value.Trim().Split(",").Select(x => { return x.Trim(); }).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        [IgnoreProperty]
        public List<int> Users { get; set; }
        public string AZUsers
        {
            get => Users == null || !Users.Any() ? null : string.Join(",", Users);
            set => Users = value
                .Trim()
                .Split(",")
                .Select(x =>
                {
                    if (int.TryParse(x.Trim(), out int u))
                    {
                        return u;
                    }
                    else
                    {
                        return 0;
                    }
                }).Where(x => x > 0)
                .ToList();
        }

        [IgnoreProperty]
        public List<string> Countries { get; set; }
        public string AZCountries
        {
            get => Countries == null || !Countries.Any() ? null : string.Join(",", Countries);
            set => Countries = value == null ? null : value.Trim().Split(",").Select(x => { return x.Trim(); }).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        [IgnoreProperty]
        public List<string> Continents { get; set; }
        public string AZContinents
        {
            get => Continents == null || !Continents.Any() ? null : string.Join(",", Continents);
            set => Continents = value == null ? null : value.Trim().Split(",").Select(x => { return x.Trim(); }).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        [IgnoreProperty]
        public List<string> AllowedIPs { get; set; }
        public string AZAllowedIPs
        {
            get => AllowedIPs == null || !AllowedIPs.Any() ? null : string.Join(",", AllowedIPs);
            set
            {
                if (value == null)
                {
                    AllowedIPs = null;
                }
                else
                {
                    AllowedIPs = value.Trim().Split(",")
                        .Select(x =>
                        {
                            if (IPAddress.TryParse(x.Trim(), out IPAddress ip))
                                return ip.MapToIPv4().ToString();
                            return null;
                        })
                        .Where(x => x != null)
                        .ToList();
                }
            }
        }

        public DateTimeOffset? ReleaseDate { get; set; }

        public int Issuer { get; set; } = 0;

        public string Description { get; set; }

        public async Task<bool> IsEnabled(IServiceProvider services)
        {
            if (Disabled)
                return false;

            return CheckDate() &&
                CheckRole(services) &&
                CheckUser(services) &&
                (await CheckContinent(services)) &&
                (await CheckCountry(services)) &&
                CheckIP(services);
        }

        private bool CheckDate()
        {
            if (ReleaseDate.HasValue && ReleaseDate < DateTime.UtcNow)
                return false;

            return true;
        }

        private bool CheckRole(IServiceProvider services)
        {
            if (RequiredRoles != null && RequiredRoles.Any())
            {
                var idInfo = services.GetRequiredService<IIDInfoAccessor>();
                var sessionInfo = idInfo.GetSessionInfo();
                foreach (var role in RequiredRoles)
                {
                    if (!sessionInfo.Roles.Contains(role))
                        return false;
                }
            }

            return true;
        }

        private bool CheckUser(IServiceProvider services)
        {
            if (Users != null && Users.Any())
            {
                var idInfo = services.GetRequiredService<IIDInfoAccessor>();
                var sessionInfo = idInfo.GetSessionInfo();
                if (!Users.Contains(sessionInfo.AccountId))
                    return false;
            }

            return true;
        }

        private async Task<GeoLocation> GetGeoLocation(IServiceProvider services)
        {
            var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var locationInfo = services.GetRequiredService<ICreyService<GeoLocationQuery>>().Value;
            if (locationInfo == null || httpContext == null)
                return null;

            return await locationInfo.GetLocation(httpContext);
        }

        private async Task<bool> CheckCountry(IServiceProvider services)
        {
            if (Countries != null && Countries.Any())
            {
                var locationInfo = await GetGeoLocation(services);
                if (locationInfo == null)
                    return Countries.Contains("unknown");
                else
                {
                    return Countries.Contains(locationInfo.CountryCode);
                }
            }

            return true;
        }

        private async Task<bool> CheckContinent(IServiceProvider services)
        {
            if (Continents != null && Continents.Any())
            {
                var locationInfo = await GetGeoLocation(services);
                if (locationInfo == null)
                    return Continents.Contains("unknown");
                else
                    return Continents.Contains(locationInfo.ContinentCode);
            }

            return true;
        }

        private bool CheckIP(IServiceProvider services)
        {
            if (AllowedIPs != null && AllowedIPs.Any())
            {
                var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext;
                if (httpContext != null)
                {
                    var ipAddresses = Enumerable.Empty<IPAddress>();
                    try
                    {
                        ipAddresses = httpContext.GetRemoteIPAddresses();
                    }
                    catch (InvalidOperationException ex)
                    {
                        services.GetRequiredService<ILogger<LegacyFeatureEntry>>().LogCritical(ex, "Failed to parse IP");
                    }
                    foreach (var ipAddress in ipAddresses)
                    {
                        if (AllowedIPs.Contains(ipAddress.MapToIPv4().ToString()))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
        }

    }
}
