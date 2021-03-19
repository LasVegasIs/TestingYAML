using System;
using Xunit;
using static Crey.FeatureControl.GeoLocationService;
using Xunit.Asserts.Compare;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Crey.FeatureControl
{
    public class FeaturesGatesTests
    {
        [Fact]
        public void MapMapEqual()
        {
            var from = new FeatureEntryTableEntity
            {
                AllowedIPs = "42.42.0.0/16,33.33.33.33",
                Countries = "EN,US",
                Description = "Test",
                Disabled = "True",
                Timestamp = DateTimeOffset.UtcNow,
                Issuer = 42,
                ReleaseDate = DateTimeOffset.Now,
                RowKey = "SomeName",
                RequiredRoles = "Tester,CSV",
                Users = "13,123",
                PartitionKey = FeatureGateStore.GatePartitionKey,
                Continents = "Afrika"
            };
            var toFrom = from.To<FeatureEntry>().To<FeatureEntryTableEntity>();
            DeepAssert.Equal(from, toFrom);
        }
    }
}