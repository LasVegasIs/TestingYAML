#nullable enable
using Crey.Contracts;
using Crey.Extensions;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Misc;
using Crey.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    /// if you want system wide chaos or endpoint based feature gates, please consider our infrastructure support for that
    ///
    // TODO: add a way to IsAnyEnabled("Name1,Name2") for OR logic on top
    // TODO: add logic - if gate has iam info (role, userids), than it is enabled only for logged in users
    // TODO: add promql metric for fg usage (label=name)
    // TODO: allow match by name or by wildcard (if wildcard is presented than matches by it regardless of name)
    public class FeatureEntry : ITo<FeatureEntryTableEntity>
    {
        private readonly Random rnd_ = new Random();

        public string Name { get; set; } = null!;

        public bool Disabled { get; set; }
        public ushort? BasisPoint { get; set; }

        public string? Description { get; set; }
        public int Issuer { get; set; } = 0;
        public DateTimeOffset? ReleaseDate { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        // next filters are combined by AND

        public List<string>? RequiredRoles { get; set; }
        public IEnumerable<int>? Users { get; set; }


        public List<string>? Countries { get; set; }

        public List<string>? Continents { get; set; }

        public List<IPNetwork>? AllowedIPs { get; set; }

        public string ETag { get; set; } = null!;


        public async Task<bool> IsEnabled(IServiceProvider services)
        {
            if (Disabled || (BasisPoint.HasValue && BasisPoint < rnd_.Next(0, 10000)))
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

        private async Task<GeoLocation?> GetGeoLocation(IServiceProvider services)
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
            if (!AllowedIPs.IsNullOrEmpty())
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
                    if (AllowedIPs != null)
                        foreach (var ipAddress in ipAddresses.Take(1)) // first is is considered externa IP
                            foreach (var range in AllowedIPs)
                                if (range.Contains(ipAddress))
                                    return true;
                }

                return false;
            }

            return true;
        }

        public T To<T>() where T : FeatureEntryTableEntity
        {
            return (T)new FeatureEntryTableEntity
            {
                RowKey = this.Name,
                PartitionKey = FeatureGateStore.GatePartitionKey,
                RequiredRoles = this.RequiredRoles.IsNullOrEmpty() ? null : string.Join(",", this.RequiredRoles!),
                Countries = this.Countries.IsNullOrEmpty() ? null : string.Join(",", this.Countries!),
                Continents = this.Continents.IsNullOrEmpty() ? null : string.Join(",", this.Continents!),
                AllowedIPs = this.AllowedIPs.IsNullOrEmpty() ? null : string.Join(",", this.AllowedIPs.Select(x => x.ToIPString())),
                Users = this.Users.IsNullOrEmpty() ? null : string.Join(",", this.Users!),
                ReleaseDate = this.ReleaseDate,
                Issuer = this.Issuer,
                Description = this.Description,
                Disabled = this.BasisPoint.HasValue ? this.BasisPoint!.ToString() : this.Disabled.ToString(),
                Timestamp = this.Timestamp,
                ETag = this.ETag,
            };
        }
    }

    public class FeatureEntryTableEntity : TableEntity, ITo<FeatureEntry>
    {
        public string? Disabled { get; set; }


        public string? RequiredRoles { get; set; }
        public string? Countries { get; set; }
        public string? Continents { get; set; }

        public int Issuer { get; set; } = 0;

        public string? Description { get; set; }

        public string? Users { get; set; }

        public string? AllowedIPs { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }


        public T To<T>()
            where T : FeatureEntry
        {
            var (disabled, basisPoint) = Parse(this.Disabled);
            return (T)new FeatureEntry
            {
                Name = this.RowKey,
                RequiredRoles = this.RequiredRoles?.Trim().Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                Countries = this.Countries?.Trim().Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                Continents = this.Continents?.Trim().Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                Users = this.Users == null ? Enumerable.Empty<int>() : this.Users.Trim().Split(",").SelectTry<int>(int.TryParse).ToList(),
                ReleaseDate = this.ReleaseDate,
                Issuer = this.Issuer,
                Description = this.Description,
                AllowedIPs = this.AllowedIPs?.Trim().Split(",").Select(x => x.Trim()).SelectTry<IPNetwork>(CreyIPAddress.TryParseNetwork).ToList(),
                BasisPoint = basisPoint,
                Disabled = disabled,
                Timestamp = this.Timestamp,
                ETag = this.ETag,
            };
        }

        private static (bool, ushort?) Parse(string? disabled)
        {
            disabled = disabled?.Trim();
            if (disabled.IsNullOrEmpty())
            {
                return (false, null);
            }
            else if (bool.TryParse(disabled, out var parsed))
            {
                return (parsed, null);
            }
            else
            {
                var basisPoint = ushort.Parse(disabled!);
                if (basisPoint == 0)
                {
                    return (true, null);
                }
                basisPoint = Math.Max(basisPoint, (ushort)10000);
                if (basisPoint == 10000)
                {
                    return (false, null);
                }

                return (false, basisPoint);
            }
        }
    }
}
