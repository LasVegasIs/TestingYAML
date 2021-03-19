using Core.Crey.FeatureControl;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.FeatureControl;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.QueriableExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IAM.Areas.FeatureGates
{
    static class FeatureDetailsExtensions
    {
        public static FeatureDetail ToFeatureDetail(this LegacyFeatureEntry entry)
        {
            return new FeatureDetail
            {
                Name = entry.Name,
                Enabled = !entry.Disabled,
                RequiredRoles = entry.RequiredRoles,
                Users = entry.Users,
                Countries = entry.Countries,
                Continents = entry.Continents,
                AllowedIPs = entry.AllowedIPs,
                ReleaseDate = entry.ReleaseDate,
                LastModification = entry.Timestamp,
                Issuer = entry.Issuer,
                Description = entry.Description,
            };
        }

        public static FeatureDetail ToFeatureDetail(this FeatureEntry entry)
        {
            return new FeatureDetail
            {
                Name = entry.Name,
                Enabled = !entry.Disabled,
                RequiredRoles = entry.RequiredRoles,
                Users = entry.Users.ToList(),
                Countries = entry.Countries,
                Continents = entry.Continents,
                AllowedIPs = entry.AllowedIPs?.Select(x=> x.ToIPString()).ToList(),
                ReleaseDate = entry.ReleaseDate,
                LastModification = entry.Timestamp,
                Issuer = entry.Issuer,
                Description = entry.Description,
            };
        }

        public static void Update(this LegacyFeatureEntry entry, FeatureUpdateParams prms)
        {
            if (prms.Enabled.HasValue)
                entry.Disabled = !prms.Enabled.Value;
            if (prms.RequiredRoles != null)
                entry.RequiredRoles = prms.RequiredRoles;
            if (prms.Users != null)
                entry.Users = prms.Users;
            if (prms.Countries != null)
                entry.Countries = prms.Countries;
            if (prms.Continents != null)
                entry.Continents = prms.Continents;
            if (prms.AllowedIPs != null)
                entry.AllowedIPs = prms.AllowedIPs;
            if (prms.Description != null)
                entry.Description = prms.Description;
        }

        public static void Update(this FeatureEntry entry, FeatureUpdateParams prms)
        {
            if (prms.Enabled.HasValue)
                entry.Disabled = !prms.Enabled.Value;
            if (prms.RequiredRoles != null)
                entry.RequiredRoles = prms.RequiredRoles;
            if (prms.Users != null)
                entry.Users = prms.Users;
            if (prms.Countries != null)
                entry.Countries = prms.Countries;
            if (prms.Continents != null)
                entry.Continents = prms.Continents;
            if (prms.AllowedIPs != null)
                entry.AllowedIPs = prms.AllowedIPs.Select(CreyIPAddress.ParseNetwork).ToList();
            if (prms.Description != null)
                entry.Description = prms.Description;
        }
    }

    public class FeatureManagerRepository
    {
        private readonly ICreyService<FeatureGateStore> featureStore_;
        private readonly bool newGates_;
        private readonly IIDInfoAccessor idInfo_;


        public FeatureManagerRepository(
            IIDInfoAccessor idInfo,
            ICreyService<FeatureGateStore> featureStore, 
            IConfiguration config)
        {
            idInfo_ = idInfo;
            featureStore_ = featureStore;
            newGates_ = config.GetValue("NewFeatureGates", false);
        }

        public async Task<PagedListResult<FeatureDetail>> GetFeatures(string continuationToken)
        {
            if (newGates_)
            {
                var table = featureStore_.Value.Storage;
                var query = new Microsoft.Azure.Cosmos.Table.TableQuery<FeatureEntryTableEntity>()
                    .Where(Microsoft.Azure.Cosmos.Table.TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, FeatureGateStore.GatePartitionKey));
                var token = new CosmosTableBasedContinuationToken(continuationToken);

                return await table.PaginateListResultAsync(query, 30, x => Task.FromResult(x.To<FeatureEntry>().ToFeatureDetail()), token);
            }
            else
            {
                var table = featureStore_.Value.GatesTable.Table;
                var query = new TableQuery<LegacyFeatureEntry>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, FeatureGateStore.GatePartitionKey));
                var token = new CloudTableBasedContinuationToken(continuationToken);

                return await table.PaginateListResult(query, x => x.ToFeatureDetail(), token);
            }

        }

        public async Task<FeatureDetail> GetFeaturesbyName(string name)
        {
            if (newGates_)
            {
                var table = featureStore_.Value.Storage;
                FeatureEntry feature = (await table.RetrieveAsync<FeatureEntryTableEntity>(FeatureGateStore.GatePartitionKey, name)).To<FeatureEntry>();
                if (feature == null)
                {
                    throw new ItemNotFoundException($"Feature gate [{name}] not found");
                }


                return feature.ToFeatureDetail();
            }
            else
            {
                var table = featureStore_.Value.GatesTable;
                var feature = await table.GetRowAsync<LegacyFeatureEntry>(FeatureGateStore.GatePartitionKey, name);
                if (feature == null)
                {
                    throw new ItemNotFoundException($"Feature gate [{name}] not found");
                }

                return feature.ToFeatureDetail();
            }
        }

        public async Task<FeatureDetail> CreateFeature(string name, FeatureUpdateParams prms)
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            if (!sessionInfo.IsUser)
                throw new AccessDeniedException($"Login required");

            if (newGates_)
            {
                var entry = new FeatureEntry()
                {
                    Name = name,
                    Issuer = sessionInfo.AccountId,
                    Disabled = true,
                };

                entry.Update(prms);

                try
                {
                    var tableResult = await featureStore_.Value.Storage.ExecuteAsync(Microsoft.Azure.Cosmos.Table.TableOperation.Insert(entry.To<FeatureEntryTableEntity>()));
                    var newEntry = (FeatureEntryTableEntity)tableResult.Result;
                    return newEntry.To<FeatureEntry>().ToFeatureDetail();
                }
                catch (Microsoft.Azure.Cosmos.Table.StorageException e) when (e.RequestInformation.HttpStatusCode == 409)
                {
                    throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Faild to create feature, already created");
                }
            }
            else
            {
                var entry = new LegacyFeatureEntry()
                {
                    PartitionKey = FeatureGateStore.GatePartitionKey,
                    Name = name,
                    Issuer = sessionInfo.AccountId,
                    Disabled = true,
                };

                entry.Update(prms);

                try
                {
                    TableResult tableResult = await featureStore_.Value.GatesTable.Table.ExecuteAsync(TableOperation.Insert(entry));
                    var newEntry = (LegacyFeatureEntry)tableResult.Result;
                    return newEntry.ToFeatureDetail();
                }
                catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 409)
                {
                    throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Faild to create feature, already created");
                }
            }
        }

        public async Task DeleteFeature(string name)
        {
            if (newGates_)
            {
                var table = featureStore_.Value.Storage;
                var feature = await table.RetrieveAsync<FeatureEntryTableEntity>(FeatureGateStore.GatePartitionKey, name);
                if (feature == null)
                {
                    throw new ItemNotFoundException($"Feature gate [{name}] not found");
                }

                try
                {
                    var tableResult = await table.ExecuteAsync(Microsoft.Azure.Cosmos.Table.TableOperation.Delete(feature));
                }
                catch (Microsoft.Azure.Cosmos.Table.StorageException e) when (e.RequestInformation.HttpStatusCode == 409)
                {
                    throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Faild to delete feature, already removed or modified");
                }
            }
            else
            {
                var table = featureStore_.Value.GatesTable;
                var feature = await table.GetRowAsync<LegacyFeatureEntry>(FeatureGateStore.GatePartitionKey, name);
                if (feature == null)
                {
                    throw new ItemNotFoundException($"Feature gate [{name}] not found");
                }

                try
                {
                    TableResult tableResult = await table.Table.ExecuteAsync(TableOperation.Delete(feature));
                }
                catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 409)
                {
                    throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Faild to delete feature, already removed or modified");
                }
            }
        }

        public async Task<FeatureDetail> UpdateFeature(string name, FeatureUpdateParams prms)
        {
            if (newGates_)
            {
                var table = featureStore_.Value.Storage;
                FeatureEntry entry = (await table.RetrieveAsync<FeatureEntryTableEntity>(FeatureGateStore.GatePartitionKey, name)).To<FeatureEntry>();
                if (entry == null)
                {
                    throw new ItemNotFoundException($"Feature gate [{name}] not found");
                }

                entry.Update(prms);

                try
                {
                    var tableResult = await featureStore_.Value.Storage.ExecuteAsync(Microsoft.Azure.Cosmos.Table.TableOperation.Replace(entry.To<FeatureEntryTableEntity>()));
                    var newEntry = (FeatureEntryTableEntity)tableResult.Result;
                    return newEntry.To<FeatureEntry>().ToFeatureDetail();
                }
                catch (Microsoft.Azure.Cosmos.Table.StorageException e) when (e.RequestInformation.HttpStatusCode == 409)
                {
                    throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Faild to update feature, already removed or modified");
                }
            }
            else
            {
                var table = featureStore_.Value.GatesTable;
                var entry = await table.GetRowAsync<LegacyFeatureEntry>(FeatureGateStore.GatePartitionKey, name);
                if (entry == null)
                {
                    throw new ItemNotFoundException($"Feature gate [{name}] not found");
                }

                entry.Update(prms);

                try
                {
                    TableResult tableResult = await featureStore_.Value.GatesTable.Table.ExecuteAsync(TableOperation.Replace(entry));
                    var newEntry = (LegacyFeatureEntry)tableResult.Result;
                    return newEntry.ToFeatureDetail();
                }
                catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 409)
                {
                    throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Faild to update feature, already removed or modified");
                }
            }
        }
    }
}
