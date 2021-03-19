using Core.Azure;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    //todo: add caching  with a timeout if IP reports too many requests
    //  ex in a table storage store all the ip responses and if it is not old, (ex 1 day) use the cached value
    //  also it is "global", no need to have a timestamp
    [Obsolete("Use GeoLocationService")]
    public class GeoLocationQuery
    {
        class AZGeoLocation : TableEntity
        {
            public string Raw { get; set; }
            public DateTimeOffset Issued { get; set; }
        }

        private const int DAY_TO_LIVE = 3;

        private readonly ILogger logger_;
        private readonly GeoLocationService geoLocationService_;
        private readonly string apiKey_;
        private readonly IHttpClientFactory httpClientFactory_;
        private readonly bool newGates_;
        private readonly TableStorage ipCacheTable_;

        public GeoLocationQuery(IConfiguration configuration,
            ILogger<GeoLocationQuery> logger,
            IHttpClientFactory httpClientFactory,
            GeoLocationService geoLocationService)
        {
            logger_ = logger;
            geoLocationService_ = geoLocationService;
            apiKey_ = configuration.GetValue<string>("IPDATA-API-KEY");
            httpClientFactory_ = httpClientFactory;
            newGates_ = configuration.GetValue("NewFeatureGates", false);

            var slot = configuration.GetDeploymentSlot().ToString().ToLower();
            var slotName = slot.ToString().ToLower();
            var connection = configuration.GetValue<string>("SAFeatureGate");
            var storageAccount = CloudStorageAccount.Parse(connection);
            var tableClient = TableStorageHelpers.CreateClient(storageAccount);
            ipCacheTable_ = new TableStorage(tableClient, $"{slotName}IpCache", true);
        }

        public async Task<GeoLocation> GetLocation(HttpContext httpContext)
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

        public async Task<GeoLocation> GetLocation(string ipAddress)
        {
            return CreyIPAddress.TryParse(ipAddress, out var ip) ? await GetLocation(ip) : null;
        }

        public async Task<GeoLocation> GetLocation(IPAddress ipAddress)
        {
            if (newGates_)
            {
                var geo = await geoLocationService_.GetLocation(ipAddress);
                return geo == null ? null : new GeoLocation(geo.YourIp, geo.ContinentCode, geo.CountryCode, geo.Latitude, geo.Longitude);
            }

            if (ipAddress.IsInternal())
            {
                return null;
            }

            var ipString = ipAddress.ToString();
            var row = await ipCacheTable_.GetRowAsync("ip", ipString);

            //check cache date
            if (row != null)
            {
                var date = row.Properties["Issued"].DateTimeOffsetValue;
                // TODO: move to configuration
                var limit = DateTime.UtcNow - TimeSpan.FromDays(DAY_TO_LIVE);
                if (!date.HasValue || date.Value < limit)
                    row = null;
            }

            string responseString;
            if (row == null)
            {
                string url = $"https://api.ipdata.co/{ipString}?api-key={apiKey_}";
                try
                {
                    var httpClient = httpClientFactory_.CreateClient();
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        // for private ip adress it gives 400 with body `{"message": "10.65.1.37 is a private IP address"}`
                        logger_.LogInformation($"Failed to get geo-location for {ipString}");
                        return null;
                    }
                    responseString = await response.Content.ReadAsStringAsync();
                    logger_.LogDebug($"ip response for {ipString}: {responseString}");
                }
                catch (Exception ex)
                {
                    logger_.LogCritical(ex, $"Failed to get geo-location for {ipString}");
                    return null;
                }

                // TODO: store parsed value (prevents storing bad values in cache and faster to response)
                await ipCacheTable_.Table.ExecuteAsync(
                    TableOperation.InsertOrReplace(
                        new AZGeoLocation
                        {
                            PartitionKey = "ip",
                            RowKey = ipString,
                            Raw = responseString,
                            Issued = DateTime.UtcNow
                        }));
            }
            else
            {
                responseString = row.Properties["Raw"].StringValue;
            }

            JObject geoJson = JObject.Parse(responseString);
            try
            {
                return new GeoLocation
                (
                    ipString,
                    geoJson["continent_code"].Value<string>(),
                    geoJson["country_code"].Value<string>(),
                    geoJson["latitude"].Value<double>(),
                    geoJson["longitude"].Value<double>()
                );
            }
            catch (Exception ex)
            {
                logger_.LogWarning(ex, "Failed to parse {GeoJson}", responseString);
                return null;
            }

        }
    }
}
