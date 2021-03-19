using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.Data.Azure
{
    public enum MigrationStatus
    {
        Migrating,
        Done,
        Error,
        InternalError,
    }

    public class MigrationInfo : TableEntity
    {
        public string Id { get; set; }

        [IgnoreProperty]
        public MigrationStatus Status { get; set; }
        public int AZStatus { get => (int)Status; set => Status = (MigrationStatus)value; }

        public string Error { get; set; }
    }

    public class MigrationResult
    {
        public MigrationStatus Status { get; internal set; }
        public MigrationInfo Info { get; internal set; }
        public string Error { get; internal set; }
    }

    public class CosmosMigrationTableStorage : CosmosTypedTableStorage<MigrationInfo>
    {
        public CosmosMigrationTableStorage(StrongCloudTableClient tableClient, AzureContextOptions options, string name)
            : base(tableClient, options, name, true)
        {
        }

        private static byte Hash(string id)
        {
            // GetHashCode is not deterministic: https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
            byte hash = 0;
            var bytes = Encoding.ASCII.GetBytes(id);
            foreach (var i in bytes)
            {
                hash += i;
            }
            return hash;
        }

        public async Task<MigrationResult> StartMigration(ILogger logger, string id)
        {
            var partition = Hash(id).ToString();
            return await StartMigration(logger, partition, id, id);
        }

        private async Task<MigrationResult> StartMigration(ILogger logger, string partition, string row, string id)
        {
            double delay = 1;
            int iteration = 10;
            while (iteration > 0)
            {
                if (delay > 1)
                {
                    // "exponential" backoff
                    await Task.Delay((int)delay);
                }
                delay *= 1.5;

                var query = Query().Where(
                   TableQuery.CombineFilters(
                       TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition),
                       TableOperators.And,
                       TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row)
                   ));
                var log = (await ExecuteQueryAsync(query)).FirstOrDefault();

                if (log == null)
                {
                    //start new migration
                    log = new MigrationInfo
                    {
                        PartitionKey = partition,
                        RowKey = row,
                        Id = id,
                        Status = MigrationStatus.Migrating,
                    };

                    try
                    {
                        var result = await Table.ExecuteAsync(TableOperation.Insert(log));
                        return new MigrationResult
                        { Status = MigrationStatus.Migrating, Info = log };
                    }
                    catch (StorageException ex)
                    {
                        logger.LogError($"Migration start error: {ex}");
                        //todo retry only on "item already exists" error
                    }
                }
                else
                {
                    if (log.Status == MigrationStatus.Migrating)
                        continue;
                    else if (log.Status == MigrationStatus.Done)
                        return new MigrationResult
                        { Status = MigrationStatus.Done, Info = log };
                    else if (log.Status == MigrationStatus.Error)
                        return new MigrationResult
                        { Status = log.Status, Info = log };
                    else
                        return new MigrationResult
                        { Status = MigrationStatus.InternalError, Info = log, Error = $"Invalid status: {log.Status}" };
                }
            }

            return new MigrationResult
            { Status = MigrationStatus.InternalError, Error = "Retry count exceeds limit" };
        }

        public async Task SkipMigration(ILogger logger, string id)
        {
            var partition = Hash(id).ToString();
            await SkipMigration(logger, partition, id, id);
        }

        private async Task SkipMigration(ILogger logger, string partition, string row, string id)
        {
            var log = new MigrationInfo
            {
                PartitionKey = partition,
                RowKey = row,
                Id = id,
                Status = MigrationStatus.Done,
            };
            await Table.ExecuteAsync(TableOperation.Insert(log));
        }

        public async Task MigrationDone(ILogger logger, MigrationResult
 result)
        {
            Trace.Assert(result.Status == MigrationStatus.Migrating);
            result.Info.Status = MigrationStatus.Done;
            await Table.ExecuteAsync(TableOperation.Replace(result.Info));
        }

        public async Task MigrationFailed(ILogger logger, MigrationResult
 result, string error)
        {
            Trace.Assert(result.Status == MigrationStatus.Migrating);
            result.Info.Status = MigrationStatus.Error;
            result.Info.Error = error ?? "General error";
            await Table.ExecuteAsync(TableOperation.Replace(result.Info));
        }
    }
}