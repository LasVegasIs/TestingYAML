using Core.Azure;
using Core.Crey.FeatureControl;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.FeatureControl;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web;
using IAM.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace IAM.Areas.FeatureGates
{
    public class FeatureDetail
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public List<string> RequiredRoles { get; set; }

        public List<int> Users { get; set; }

        public List<string> Countries { get; set; }

        public List<string> Continents { get; set; }

        public List<string> AllowedIPs { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

        public DateTimeOffset? LastModification { get; set; }

        public int Issuer { get; set; }

        public string Description { get; set; }
    }

    public class FeatureUpdateParams : IValidatableObject
    {

        public bool? Enabled { get; set; }

        public List<string> RequiredRoles { get; set; }

        public List<int> Users { get; set; }

        public List<string> Countries { get; set; }

        public List<string> Continents { get; set; }

        public List<string> AllowedIPs { get; set; }

        public string Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AllowedIPs != null)
            {
                foreach (var ip in AllowedIPs)
                {
                    if (!IPAddress.TryParse(ip, out IPAddress address))
                        yield return new ValidationResult($"Invalid IPAddress: [{ip}].", new[] { nameof(AllowedIPs) });
                }
            }
        }
    }


    public class Feature
    {
        public bool IsEnabled { get; set; }
    }

    public class FeatureList
    {
        public Dictionary<string, bool> Items { get; set; }
    }

    public class FeatureQuery
    {
        public List<string> Features { get; set; }
    }

    [EnableCors]
    [ApiController]
    public class FeatureController : ControllerBase
    {
        private readonly ICreyService<IFeatureGate> gates_;
        private readonly ICreyService<FeatureManagerRepository> featureGates_;
        private readonly ICreyService<GeoLocationQuery> location_;

        public FeatureController(
            ICreyService<IFeatureGate> gates,
            ICreyService<GeoLocationQuery> location,
            ICreyService<FeatureManagerRepository> featureGates)
        {
            gates_ = gates;
            location_ = location;
            featureGates_ = featureGates;
        }

        [HttpGet("featuregates/api/v1/geolocation")]
        public async Task<ActionResult<GeoLocation>> GetLocation()
        {
            var loc = await location_.Value.GetLocation(HttpContext);
            if (loc == null)
                throw new ItemNotFoundException("Could not get location");
            return loc;
        }

        [HttpPost("featuregates/api/v1/query")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<FeatureList>> GetFeature(FeatureQuery query)
        {
            var items = await gates_.Value.GetFeaturesAsync(query.Features);
            return new FeatureList { Items = items };
        }

        [HttpGet("featuregates/api/v1/features/{name}/status")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<Feature>> GetFeature(string name)
        {
            bool isEnabled = await gates_.Value.IsFeatureEnabledAsync(name);
            return new Feature { IsEnabled = isEnabled };
        }

        [HttpGet("featuregates/adminapi/v1/features")]
        [HttpGet("featuregates/api/v1/features", Order = RestApiDefaults.DeprecatedRouteOrder)]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = Roles.FeatureManager)]
        public Task<PagedListResult<FeatureDetail>> GetFeatureGates(string continuationToken)
        {
            return featureGates_.Value.GetFeatures(continuationToken);
        }

        [HttpGet("featuregates/adminapi/v1/features/{name}")]
        [HttpGet("featuregates/api/v1/features/{name}", Order = RestApiDefaults.DeprecatedRouteOrder)]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = Roles.FeatureManager)]
        public Task<FeatureDetail> GetFeatureGateByName(string name)
        {
            return featureGates_.Value.GetFeaturesbyName(name);
        }

        [HttpPost("featuregates/adminapi/v1/features/{name}")]
        [HttpPost("featuregates/api/v1/features/{name}", Order = RestApiDefaults.DeprecatedRouteOrder)]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = Roles.FeatureManager)]
        public Task<FeatureDetail> CreateFeature(string name, [FromBody] FeatureUpdateParams prms)
        {
            return featureGates_.Value.CreateFeature(name, prms);
        }

        [HttpDelete("featuregates/adminapi/v1/features/{name}")]
        [HttpDelete("featuregates/api/v1/features/{name}", Order = RestApiDefaults.DeprecatedRouteOrder)]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = Roles.FeatureManager)]
        public async Task<ActionResult> DeleteFeature(string name)
        {
            await featureGates_.Value.DeleteFeature(name);
            return Ok();
        }

        [HttpPut("featuregates/adminapi/v1/features/{name}")]
        [HttpPut("featuregates/api/v1/features/{name}", Order = RestApiDefaults.DeprecatedRouteOrder)]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = Roles.FeatureManager)]
        public Task<FeatureDetail> UpdateFeature(string name, [FromBody] FeatureUpdateParams prms)
        {
            return featureGates_.Value.UpdateFeature(name, prms);
        }


        [HttpPut("featuregates/adminapi/v1/features/migrate")]        
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = Roles.FeatureManager)]
        public async Task MigrateFeatureGatesToCosmos(
            [FromServices]EventualCloudTableClient storage,
            [FromServices]IConfiguration configuration,
            [FromServices]ServiceOption serviceOption){
                
            // data is small and not online, so next tech is ok for this
            // also it is out of azcontext code, so just do it here
            var slot = configuration.GetDeploymentSlot().ToString().ToLower();
            var slotName = slot.ToString().ToLower();
            var connection = configuration.GetValue<string>("SAFeatureGate");
            var storageAccount = CloudStorageAccount.Parse(connection);
            var tableClient = TableStorageHelpers.CreateClient(storageAccount);
            var newTable = storage.GetTableReference($"{slotName}Gates");
            await newTable.CreateIfNotExistsAsync();
            var gatesTable = new TableStorage(tableClient, $"{slotName}Gates", true);
            var data= await gatesTable.ExecuteQueryAsync(new TableQuery<LegacyFeatureEntry>());
            foreach (var item in data){
                var newEntry = new FeatureEntryTableEntity {
                    AllowedIPs = item.AZAllowedIPs,
                    Continents = item.AZContinents,
                    Countries = item.AZCountries,
                    Description = item.Description,
                    Disabled = item.Disabled.ToString(),
                    PartitionKey = item.PartitionKey,
                    Issuer=  item.Issuer,
                    ReleaseDate = item.ReleaseDate,
                    RequiredRoles = item.AZRequiredRoles,
                    RowKey = item.RowKey,
                    Users = item.AZUsers,                
                };
                await newTable.InsertOrMergeAsync(newEntry);
            }
        }
        
    }
}