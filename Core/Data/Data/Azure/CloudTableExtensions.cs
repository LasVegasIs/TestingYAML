#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.Data.Azure
{
    public static class CloudTableExtensions
    {
        public static async Task<PagedListResult<TOutput>> PaginateListResultAsync<TInput, TOutput>(
              this Microsoft.Azure.Cosmos.Table.CloudTable table,
              Microsoft.Azure.Cosmos.Table.TableQuery<TInput> query,
              int pageSize,
              Func<TInput, Task<TOutput>> conversion,
              TableBasedContinuationToken continuationToken,
              CancellationToken cancelationToken = default(CancellationToken))
              where TInput : Microsoft.Azure.Cosmos.Table.ITableEntity, new()
        {
            var items = new List<TOutput>();
            var token = continuationToken.IsContinuation ? continuationToken.TableCursor : null;
            var oc = new Microsoft.Azure.Cosmos.Table.OperationContext();
            var ro = new Microsoft.Azure.Cosmos.Table.TableRequestOptions();

            query = query.Take(pageSize);

            var seg = await table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
            continuationToken.TableCursor = seg.ContinuationToken;
            items.AddRange(
                (await Task.WhenAll(seg.Select(async x => await conversion(x))))
                .Where(x => x != null)
            );

            return new PagedListResult<TOutput> { Items = items, ContinuationToken = continuationToken?.ToString() };
        }
    }
}
