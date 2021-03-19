using Core.Azure;
using Core.Crey.FeatureControl;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Kernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crey.FeatureControl
{
    public class FeatureGateStore
    {
        public const string GatePartitionKey = "gate";
        public const string ChaosPartitionKey = "chaos";

        private readonly Random rnd_;
        public readonly Microsoft.Azure.Cosmos.Table.CloudTable Storage;
        private readonly ILogger<FeatureGateStore> logger_;
        private readonly string Service;
        private bool newGates_;

        [Obsolete("Usage Storage")]
        public TableStorage GatesTable { get; private set; }

        public FeatureGateStore(
            IConfiguration configuration,
            ILogger<FeatureGateStore> logger,
            ServiceOption serviceOption,
            EventualCloudTableClient storage)
        {
            var slot = configuration.GetDeploymentSlot().ToString().ToLower();
            var slotName = slot.ToString().ToLower();
            var connection = configuration.GetValue<string>("SAFeatureGate");
            newGates_ = configuration.GetValue("NewFeatureGates", false);
            var storageAccount = CloudStorageAccount.Parse(connection);
            var tableClient = TableStorageHelpers.CreateClient(storageAccount);
            Storage = storage.GetTableReference($"{slotName}Gates");
            Storage.CreateIfNotExists();
            logger_ = logger;
            rnd_ = new Random();
            GatesTable = new TableStorage(tableClient, $"{slotName}Gates", true);
            Service = serviceOption.Service;
        }

        public async Task<bool> IsFeatureEnabledAsync(string name, IServiceProvider services)
        {
            try
            {
                if (newGates_)
                {
                    var feature = (await Storage.RetrieveAsync<FeatureEntryTableEntity>(GatePartitionKey, name)).To<FeatureEntry>();
                    return feature != null && await feature.IsEnabled(services);
                }
                else
                {
                    var feature = await GatesTable.GetRowAsync<LegacyFeatureEntry>(GatePartitionKey, name);
                    return feature != null && await feature.IsEnabled(services);
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<Dictionary<string, bool>> GetFeaturesAsync(IEnumerable<string> toQuery, IServiceProvider services)
        {
            return toQuery
                .Distinct()
                .Batch(20)
                    .SelectTo(services, GetFeaturesBatchAsync)
                .SelectManyAsync(x => x)
                .ToDictionaryAsync();
        }

        private async Task<Dictionary<string, bool>> GetFeaturesBatchAsync(IServiceProvider services, IEnumerable<string> toQuery)
        {
            var result = toQuery.ToDictionary(x => x, x => false);
            if (!result.Any())
                return result;

            if (newGates_)
            {
                return await Get(services, result);
            }
            else
            {
                return await GetLegacy(services, result);
            }
        }

        private async Task<Dictionary<string, bool>> Get(IServiceProvider services, Dictionary<string, bool> result)
        {
            string filter = null;
            foreach (var name in result.Keys)
            {
                var f = Microsoft.Azure.Cosmos.Table.TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, name);
                if (filter == null)
                {
                    filter = f;
                }
                else
                {
                    filter = Microsoft.Azure.Cosmos.Table.TableQuery.CombineFilters(filter, Microsoft.Azure.Cosmos.Table.TableOperators.Or, f);
                }
            }

            var query = new Microsoft.Azure.Cosmos.Table.TableQuery<FeatureEntryTableEntity>()
                .Where(
                    Microsoft.Azure.Cosmos.Table.TableQuery.CombineFilters(
                        Microsoft.Azure.Cosmos.Table.TableQuery.GenerateFilterCondition("PartitionKey", Microsoft.Azure.Cosmos.Table.QueryComparisons.Equal, GatePartitionKey),
                        Microsoft.Azure.Cosmos.Table.TableOperators.And,
                        filter)
                );

            var results = await Storage.ExecuteQuerySegmentedAsync(query);
            foreach (var featureRow in results)
            {
                try
                {
                    var feature = featureRow.To<FeatureEntry>();
                    result[feature.Name] = feature != null && await feature.IsEnabled(services);
                }
                catch (Exception ex)
                {
                    logger_.LogCritical(ex, "FG storage has badly formed featreu : {FeatureName}", featureRow.RowKey);
                    result[featureRow.RowKey] = false;
                }
            }
            return result;
        }

        [Obsolete]
        private async Task<Dictionary<string, bool>> GetLegacy(IServiceProvider services, Dictionary<string, bool> result)
        {
            string filter = null;
            foreach (var name in result.Keys)
            {
                var f = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, name);
                if (filter == null)
                {
                    filter = f;
                }
                else
                {
                    filter = TableQuery.CombineFilters(filter, TableOperators.Or, f);
                }
            }

            var query = new TableQuery<LegacyFeatureEntry>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, GatePartitionKey),
                        TableOperators.And,
                        filter)
                );
            await GatesTable.ProcessQueryAsync(query,
                async feature =>
                {
                    try
                    {
                        result[feature.RowKey] = feature != null && await feature.IsEnabled(services);
                    }
                    catch (Exception ex)
                    {
                        logger_.LogCritical(ex, $"FG query failed: {feature.RowKey}");
                        result[feature.RowKey] = false;
                    }
                });
            return result;
        }

        [Obsolete]
        public async Task<bool> IsChaosAsync(string route, IServiceProvider services)
        {
            try
            {
                var query = new TableQuery<ChaosEntry>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ChaosPartitionKey),
                            TableOperators.And,
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, Service),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, $"{Service}z")
                            )
                        )
                    );
                var lst = await GatesTable.ExecuteQueryAsync(query);
                lst.Sort((a, b) => a.Order.CompareTo(b.Order));
                foreach (var entry in lst)
                {
                    if (entry.IsChaos(route, rnd_, services))
                    {
                        logger_.LogInformation("Chaos {} triggered for: {}", entry.RowKey, route);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
