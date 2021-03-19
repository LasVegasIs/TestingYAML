using System;
using Xunit;
using static Crey.FeatureControl.GeoLocationService;
using Xunit.Asserts.Compare;

namespace Crey.FeatureControl
{
    public class GeoLocationServiceTests
    {
        [Fact]
        public void MapMapEqual()
        {
            var from = new IpGeo { ContinentCode = "a", CountryCode = "b", Latitude = 1, Longitude = 2, YourIp = "0.3.0.0", Timestamp = DateTimeOffset.UtcNow };
            var toFrom = from.To<IpGeoTableEntity>().To<IpGeo>();
            DeepAssert.Equal(from, toFrom);
        }
    }
}