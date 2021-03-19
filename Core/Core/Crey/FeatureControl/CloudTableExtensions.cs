using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Crey.FeatureControl
{
    // TODO: move to common code when used outside for FGs
    public static class CloudTableExtensions
    {

        // in C# 9 can type T?
        public static async Task<T> RetrieveAsync<T>(this CloudTable table, string partitionKey, string rowKey)
            where T : ITableEntity
        {
            return await TableOperation
                                .Retrieve<T>(partitionKey, rowKey)
                                .To(table.ExecuteAsync)
                                .ToAsync(x => x.HttpStatusCode == 404 ? default : x.HttpStatusCode >= 400 ? throw new StorageException($"Failed to get {table.Name}.{partitionKey}.{rowKey} with {x.HttpStatusCode}") : (T)x.Result);
        }

        public static async Task<T> InsertOrMergeAsync<T>(this CloudTable table, T entity)
            where T : ITableEntity
        {
            return await TableOperation
                                .InsertOrMerge(entity)
                                .To(table.ExecuteAsync)
                                .ToAsync(x => x.HttpStatusCode >= 400 ? throw new StorageException($"Failed to upsert {entity.PartitionKey}.{entity.RowKey} with {x.HttpStatusCode}") : (T)x.Result);
        }

        public static async Task<IEnumerable<T>> ExecuteQuerySegmentedAsync<T>(this CloudTable table, TableQuery<T> query, System.Threading.CancellationToken cancellationToken = default)
            where T : ITableEntity, new()
        {
            var result = new List<T>();
            TableContinuationToken token = default;
            do
            {
                var batch = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = batch.ContinuationToken;
                result.AddRange(batch.Results);
            }
            while (token != default && !cancellationToken.IsCancellationRequested);
            return result;
        }
    }
}
