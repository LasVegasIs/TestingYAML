using Core.Extensions.ThrowIf;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// note: moved to net50
namespace Core.Azure
{
    public static class CosmosTableStorageHelpers
    {
        public static StrongCloudTableClient CreateClient(CloudStorageAccount saConnection)
        {
            var tableClient = new StrongCloudTableClient(saConnection.TableStorageUri, saConnection.Credentials);
            tableClient.DefaultRequestOptions = new TableRequestOptions
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(15),
                ServerTimeout = TimeSpan.FromSeconds(10),
            };

            return tableClient;
        }
    }

    public class CosmosTableStorage
    {
        public CloudTable Table { get; }

        public CosmosTableStorage(StrongCloudTableClient tableClient, string tableName)
        {
            tableName_ = tableName;
            Table = tableClient.GetTableReference(tableName_);
        }

        public CosmosTableStorage(StrongCloudTableClient tableClient, string tableName, bool ensureExists)
        {
            tableName_ = tableName;

            Table = tableClient.GetTableReference(tableName_);

            var exists = Table.ExistsAsync().Result;
            if (!exists)
            {
                if (ensureExists)
                {
                    Table.CreateIfNotExistsAsync().Wait();
                }
                else
                {
                    throw new Exception($"table does not exist and creation not allowed. name:[{tableName}]");
                }
            }
        }

        private readonly string tableName_;

        // remove and recreate table
        public async Task ClearByRecreate()
        {
            // todo: make it asnyc
            await Table.DeleteIfExistsAsync();
            await Table.CreateAsync();
        }


        public async Task<Entity> GetRowAsync<Entity>(string partitionKey, string rowKey)
            where Entity : ITableEntity, new()
        {
            var query = new TableQuery<Entity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey)
                  ));
            var lst = await ExecuteQueryAsync(query);
            return lst.FirstOrDefault();
        }

        public Task<DynamicTableEntity> GetRowAsync(string partitionKey, string rowKey)
        {
            return GetRowAsync<DynamicTableEntity>(partitionKey, rowKey);
        }

        public async Task<List<DynamicTableEntity>> ExecuteQueryAsync(TableQuery query, CancellationToken cancelationToken)
        {
            var items = new List<DynamicTableEntity>();
            TableContinuationToken token = null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            do
            {
                var seg = await Table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                //if (onProgress != null) onProgress(items);

            } while (token != null && !cancelationToken.IsCancellationRequested);

            return items;
        }

        public Task<List<DynamicTableEntity>> ExecuteQueryAsync(TableQuery query) =>
            ExecuteQueryAsync(query, default);

        public Task ProcessQueryAsync(TableQuery query, Action<DynamicTableEntity> action, CancellationToken cancelationToken = default(CancellationToken))
        {
            return ProcessQueryAsync(query, entry => { action(entry); return Task.CompletedTask; }, cancelationToken);
        }

        public async Task ProcessQueryAsync(TableQuery query, Func<DynamicTableEntity, Task> action, CancellationToken cancelationToken = default(CancellationToken))
        {
            TableContinuationToken token = null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            do
            {
                var seg = await Table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
                token = seg.ContinuationToken;
                foreach (var entry in seg)
                {
                    await action(entry);
                }

            } while (token != null && !cancelationToken.IsCancellationRequested);
        }

        public Task ProcessQueryAsync<T>(TableQuery<T> query, Action<T> action, CancellationToken cancelationToken = default(CancellationToken))
            where T : ITableEntity, new()
        {
            return ProcessQueryAsync<T>(query, entry => { action(entry); return Task.CompletedTask; }, cancelationToken);
        }

        public async Task ProcessQueryAsync<T>(TableQuery<T> query, Func<T, Task> action, CancellationToken cancelationToken = default(CancellationToken))
            where T : ITableEntity, new()
        {
            TableContinuationToken token = null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            do
            {
                var seg = await Table.ExecuteQuerySegmentedAsync<T>(query, token, ro, oc, cancelationToken);
                token = seg.ContinuationToken;
                foreach (var entry in seg)
                {
                    await action(entry);
                }

            } while (token != null && !cancelationToken.IsCancellationRequested);
        }

        public async Task DeleteQueryAsync(TableQuery query, CancellationToken cancelationToken = default(CancellationToken))
        {
            TableContinuationToken token = null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            do
            {
                var seg = await Table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);

                TableBatchOperation tableBatchOperation = null;
                foreach (var item in seg)
                {
                    if (cancelationToken.IsCancellationRequested)
                        break;

                    if (tableBatchOperation == null)
                        tableBatchOperation = new TableBatchOperation();

                    tableBatchOperation.Add(TableOperation.Delete(item));
                    if (tableBatchOperation.Count >= 99)
                    {
                        await Table.ExecuteBatchAsync(tableBatchOperation);
                        tableBatchOperation = null;
                    }
                }

                if (tableBatchOperation != null && !cancelationToken.IsCancellationRequested)
                {
                    await Table.ExecuteBatchAsync(tableBatchOperation);
                }

                token = seg.ContinuationToken;
            } while (token != null && !cancelationToken.IsCancellationRequested);
        }

        public async Task<long> CountAsync(TableQuery query, CancellationToken cancelationToken = default(CancellationToken))
        {
            TableContinuationToken token = null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            long result = 0;

            do
            {
                var seg = await Table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
                result += seg.Count();
                token = seg.ContinuationToken;

            } while (token != null && !cancelationToken.IsCancellationRequested);

            return result;
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(TableQuery<T> query, CancellationToken cancelationToken = default(CancellationToken)) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            do
            {
                TableQuerySegment<T> seg = await Table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                //if (onProgress != null) onProgress(items);

            } while (token != null && !cancelationToken.IsCancellationRequested);

            return items;
        }
    }

    public class CosmosTypedTableStorage<Entry> : CosmosTableStorage
        where Entry : TableEntity, new()
    {
        public CosmosTypedTableStorage(StrongCloudTableClient tableClient, AzureContextOptions options)
            : base(tableClient, options.GetTableName(typeof(Entry).Name))
        {
        }

        public CosmosTypedTableStorage(StrongCloudTableClient tableClient, AzureContextOptions options, string name)
            : base(tableClient, options.GetTableName(name))
        {
        }

        public CosmosTypedTableStorage(StrongCloudTableClient tableClient, AzureContextOptions options, string name, bool createOnDemand)
            : base(tableClient, options.GetTableName(name), createOnDemand)
        {
        }

        public TableQuery<Entry> Query()
        {
            return new TableQuery<Entry>();
        }

        public Task<List<Entry>> ExecuteQueryAsync(TableQuery<Entry> query, CancellationToken cancelationToken = default(CancellationToken))
        {
            return base.ExecuteQueryAsync(query, cancelationToken);
        }
    }

    public static class CloudTableEntityExtensions
    {
        public static T ToTableEntity<T>(this DynamicTableEntity entry)
            where T : TableEntity, new()
        {
            var e = new T();
            e.PartitionKey = entry.PartitionKey;
            e.RowKey = entry.RowKey;
            e.Timestamp = entry.Timestamp;
            e.ETag = entry.ETag;
            e.ReadEntity(entry.Properties, new OperationContext());
            return e;
        }
    }
}