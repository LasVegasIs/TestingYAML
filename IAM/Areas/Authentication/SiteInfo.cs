using Crey.FeatureControl;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class SiteInfo
    {
        public string UserAgent { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
    }


    public static class SiteInfoExtensions
    {
        public static async Task<SiteInfo> GetSiteInfo(this HttpContext context)
        {
            var services = context.RequestServices;
            var session = new SiteInfo();
            session.UserAgent = context.Request.Headers["User-Agent"];

            // very paranoid version to be sure no error escape this code - required until geoloc error has not been not found
            string ipList = ""; // only for debug
            string fwdfor = "";
            ILogger logger = null; // only for debug, 
            try
            {
                var geoLoc = services.GetService<ICreyService<GeoLocationQuery>>();
                logger = context.RequestServices.GetRequiredService<ILogger<SiteInfo>>();
                IEnumerable<IPAddress> ipAddresses = context.GetRemoteIPAddresses();
                ipList = string.Join(",", ipAddresses.Select(x => x.ToString()));
                if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    fwdfor = context.Request.Headers["X-Forwarded-For"];
                }

                if (!ipAddresses.Any())
                {
                    session.Country = "";
                    session.Ip = "";
                    logger?.LogWarning($"geo location was unable to get infor for: [{fwdfor}] [{ipList}]");
                }
                else
                {
                    var ip = ipAddresses.First();
                    var loc = await geoLoc.Value.GetLocation(ip);
                    session.Country = loc.CountryCode;
                    session.Ip = loc.YourIp;
                }
            }
            catch (Exception ex)
            {
                  session.Country = "";
                  session.Ip = "";
                  logger?.LogCritical(ex, $"geo location is still not working: [{fwdfor}] [{ipList}]");
            }

            return session;
        }
    }
}
