#nullable enable
using Core.Crey.FeatureControl;
using Crey.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Misc;
using Core.Azure;
using IpData;
using IpData.Exceptions;

namespace Crey.FeatureControl
{
    /// obtains and stores all the ip responses and if it is not old, use the cached value
    public class GeoLocationService
    {
        public class IpGeo : ITo<IpGeoTableEntity>
        {
            //  ips are global and near constants, and there is limit of ip4; but ip6 growing and could have bug in area detection or other geo migrations - so need cache
            public string YourIp { get; set; } = null!;

            public DateTimeOffset Timestamp { get; set; }

            public string? ContinentCode { get; set; }
            public string? CountryCode { get; set; }

            public double Latitude { get; set; }

            public double Longitude { get; set; }

            public string? ETag { get; set; }

            public T To<T>() where T : IpGeoTableEntity
            {
                return (T)new IpGeoTableEntity
                {
                    PartitionKey = "ip",
                    RowKey = this.YourIp,
                    ContinentCode = this.ContinentCode,
                    Latitude = this.Latitude,
                    CountryCode = this.CountryCode,
                    Longitude = this.Longitude,
                    Timestamp = this.Timestamp,
                    ETag = this.ETag,
                };
            }
        }

        public class IpGeoTableEntity : TableEntity, ITo<IpGeo>
        {
            public T To<T>() where T : IpGeo
            {
                return (T)new IpGeo
                {
                    YourIp = this.RowKey,
                    ContinentCode = this.ContinentCode,
                    Latitude = this.Latitude,
                    CountryCode = this.CountryCode,
                    Longitude = this.Longitude,
                    Timestamp = this.Timestamp,
                    ETag = this.ETag,
                };
            }
            public string? ContinentCode { get; set; }
            public string? CountryCode { get; set; }

            public double Latitude { get; set; }

            public double Longitude { get; set; }
        }

        private readonly ILogger logger_;
        private readonly string apiKey_;
        private readonly IpDataClient ipdata_;
        private readonly TimeSpan cacheTimeout_;
        private readonly IHttpClientFactory httpClientFactory_;
        private CloudTable ipCacheTable_;

        public GeoLocationService(IConfiguration configuration,
            ILogger<GeoLocationQuery> logger,
            IHttpClientFactory httpClientFactory,
            EventualCloudTableClient cache)
        {
            logger_ = logger;
            apiKey_ = configuration.GetValue<string>("IPDATA-API-KEY");
            ipdata_ = new IpDataClient(apiKey_);
            cacheTimeout_ = TimeSpan.FromDays(configuration.GetValue<byte>("IPDATA-CACHE-TIMEOUT-DAYS", 3));
            httpClientFactory_ = httpClientFactory;
            var slotName = configuration.GetDeploymentSlot().ToString().ToLower();
            ipCacheTable_ = cache.GetTableReference($"{slotName}IpCache");
            ipCacheTable_.CreateIfNotExists();// making it on par with old storage based GeoLocationQuery (create table in ctor)
        }

        public async Task<IpGeo?> GetLocation(HttpContext httpContext)
        {
            var ipAddresses = Enumerable.Empty<IPAddress>();
            try
            {
                ipAddresses = httpContext.GetRemoteIPAddresses();
            }
            catch (InvalidOperationException ex)
            {
                logger_.LogCritical(ex, "Failed to parse IP");
            }

            foreach (var ip in ipAddresses)
            {
                var loc = await GetLocation(ip);
                if (loc != null)
                    return loc;
            }
            return null;
        }

        public async Task<IpGeo?> GetLocation(string ipAddress)
        {
            return CreyIPAddress.TryParse(ipAddress, out var ip) ? await GetLocation(ip) : null;
        }

        public async Task<IpGeo?> GetLocation(IPAddress ipAddress)
        {
            if (ipAddress.IsInternal())
                return null;

            var ipString = ipAddress.ToString();
            var row = await ipCacheTable_.RetrieveAsync<IpGeoTableEntity>("ip", ipString);

            if (row == null || row.Timestamp < DateTimeOffset.UtcNow - cacheTimeout_)
            {
                try
                {
                    var ipGeo = new IpGeo { YourIp = ipString }; // TODO: make use of canonical representation of ip so avoid saving it again
                    try
                    {
                        var ipInfo = await ipdata_.Lookup(ipString, x => x.Latitude, x => x.Longitude, x => x.RegionCode, x => x.CountryCode, x => x.ContinentCode);
                        ipGeo.Latitude = ipInfo.Latitude!.Value;
                        ipGeo.Longitude = ipInfo.Longitude!.Value;
                        ipGeo.ContinentCode = ipInfo.ContinentCode;
                        ipGeo.CountryCode = ipInfo.CountryCode;
                        ipGeo.ETag = row?.ETag;
                    }
                    catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
                    {
                        // for private ip address it gives 400 with body `{"message": "10.65.1.37 is a private IP address"}`
                        logger_.LogWarning("Failed to get geo-location for {ipString} with {StatusCode}", ipString, ex.StatusCode);
                        return null;
                    }

                    row = await ipCacheTable_.InsertOrMergeAsync(ipGeo.To<IpGeoTableEntity>());
                }
                catch (Exception ex)
                {
                    logger_.LogCritical(ex, $"Failed to get geo-location for {ipString}");
                    return null;
                }
            }

            return row.To<IpGeo>();
        }
    }
}
