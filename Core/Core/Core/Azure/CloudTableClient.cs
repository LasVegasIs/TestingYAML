using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;

namespace Core.Azure
{
    /// mutliregion replication and write with `lazy` consistency
    public class EventualCloudTableClient : MetricsCloudTableClient
    {
        public EventualCloudTableClient(StorageUri storageUri, StorageCredentials credentials) : base(storageUri, credentials)
        {
        }
    }

    /// single region strong consistency client (so write extensions which suits)
    public class StrongCloudTableClient : MetricsCloudTableClient
    {
        public StrongCloudTableClient(StorageUri storageUri, StorageCredentials credentials) : base(storageUri, credentials)
        {
        }
    }

    public class MetricsCloudTableClient : CloudTableClient
    {

        public MetricsCloudTableClient(StorageUri storageUri, StorageCredentials credentials) : base(storageUri, credentials)
        {
        }

        // in C# 9 can return MetricsCloudTableClient and it will work for override
        public override CloudTable GetTableReference(string tableName)
        {
            return new MetricsCloudTable(new Uri(this.StorageUri.PrimaryUri, tableName), this.Credentials, this.TableClientConfiguration);
        }
    }

    public class MetricsCloudTable : CloudTable
    {
        public static readonly Prometheus.Histogram azure_storage_table_request =
                Prometheus.Metrics.CreateHistogram(
                        nameof(azure_storage_table_request),
                        "Requests to the table storage in azure",
                        nameof(TableOperation.OperationType), nameof(CloudTable.Name));

        public MetricsCloudTable(Uri tableAddress, StorageCredentials credentials, TableClientConfiguration configuration = null) : base(tableAddress, credentials, configuration)
        {
        }


        public override async Task<TableResult> ExecuteAsync(TableOperation operation, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(operation.OperationType.ToString(), this.Name).NewTimer())
                return await base.ExecuteAsync(operation, requestOptions, operationContext, cancellationToken);
        }

        public override async Task<TableQuerySegment<DynamicTableEntity>> ExecuteQuerySegmentedAsync(TableQuery query, TableContinuationToken token, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(nameof(ExecuteQuerySegmentedAsync), this.Name).NewTimer())
                return await base.ExecuteQuerySegmentedAsync(query, token, requestOptions, operationContext, cancellationToken);
        }

        public override async Task<TableQuerySegment<TResult>> ExecuteQuerySegmentedAsync<TResult>(TableQuery query, EntityResolver<TResult> resolver, TableContinuationToken token, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(nameof(ExecuteQuerySegmentedAsync), this.Name).NewTimer())
                return await base.ExecuteQuerySegmentedAsync(query, resolver, token, requestOptions, operationContext, cancellationToken);
        }

        public override async Task<TableQuerySegment<TResult>> ExecuteQuerySegmentedAsync<TResult>(TableQuery query, EntityResolver<TResult> resolver, TableContinuationToken token, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(nameof(ExecuteQuerySegmentedAsync), this.Name).NewTimer())
                return await base.ExecuteQuerySegmentedAsync(query, resolver, token, cancellationToken);
        }

        public override async Task<TableQuerySegment<TElement>> ExecuteQuerySegmentedAsync<TElement>(TableQuery<TElement> query, TableContinuationToken token, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(nameof(ExecuteQuerySegmentedAsync), this.Name).NewTimer())
                return await base.ExecuteQuerySegmentedAsync(query, token, requestOptions, operationContext, cancellationToken);
        }

        public override async Task<TableQuerySegment<TResult>> ExecuteQuerySegmentedAsync<TElement, TResult>(TableQuery<TElement> query, EntityResolver<TResult> resolver, TableContinuationToken token, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(nameof(ExecuteQuerySegmentedAsync), this.Name).NewTimer())
                return await base.ExecuteQuerySegmentedAsync(query, resolver, token, requestOptions, operationContext, cancellationToken);
        }

        public override async Task<TableBatchResult> ExecuteBatchAsync(TableBatchOperation batch, TableRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            using (azure_storage_table_request.WithLabels(nameof(ExecuteBatchAsync), this.Name).NewTimer())
                return await base.ExecuteBatchAsync(batch, requestOptions, operationContext, cancellationToken);
        }
    }
}