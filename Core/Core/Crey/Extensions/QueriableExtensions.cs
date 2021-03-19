using Crey.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.QueriableExtensions
{
    // note: moved to net50
    public static class QueriableExtensionImpl
    {
        public static async Task<PagedListResult<TOutput>> PaginateListResultAsync<TInput, TOutput>(
            this IQueryable<TInput> queriable,
            OffsetBasedContinuationToken continuationToken,
            int pageSize,
            Func<TInput, TOutput> conversion)
        {
            int offset = Math.Clamp(continuationToken.Offset, 0, int.MaxValue);
            var data = await queriable.Skip(offset)
                     .Take(pageSize + 1)
                     .ToListAsync();
            var list = data.Select(x => conversion(x))
                     .Where(x => x != null)
                     .ToList();

            OffsetBasedContinuationToken token = null;
            if (list.Count > pageSize)
            {
                token = new OffsetBasedContinuationToken
                {
                    Offset = offset + pageSize,
                };
                list.RemoveAt(list.Count - 1); // keep it for the next page
            }
            return new PagedListResult<TOutput> { Items = list, ContinuationToken = token?.IntoToken() };
        }

        [Obsolete("Use PaginateListResultAsync")]
        public static PagedListResult<TOutput> PaginateListResult<TInput, TOutput>(
            this IQueryable<TInput> queriable,
            OffsetBasedContinuationToken continuationToken,
            int pageSize,
            Func<TInput, TOutput> conversion)
        {
            int offset = Math.Clamp(continuationToken.Offset, 0, int.MaxValue);
            var list = queriable.Skip(offset)
                     .Take(pageSize + 1)
                     .ToList()
                     .Select(x => conversion(x))
                     .Where(x => x != null)
                     .ToList();

            OffsetBasedContinuationToken token = null;
            if (list.Count > pageSize)
            {
                token = new OffsetBasedContinuationToken
                {
                    Offset = offset + pageSize,
                };
                list.RemoveAt(list.Count - 1); // keep it for the next page
            }
            return new PagedListResult<TOutput> { Items = list, ContinuationToken = token?.IntoToken() };
        }

        public static Task<PagedListResult<TOutput>> PaginateListResult<TInput, TOutput>(
            this CloudTable table,
            TableQuery<TInput> query,
            int pageSize,
            Func<TInput, TOutput> conversion,
            CloudTableBasedContinuationToken continuationToken,
            CancellationToken cancelationToken = default(CancellationToken))
            where TInput : ITableEntity, new()
        {
            return table.PaginateListResultAsync(query, pageSize, x => Task.FromResult(conversion(x)), continuationToken, cancelationToken);
        }

        public static async Task<PagedListResult<TOutput>> PaginateListResultAsync<TInput, TOutput>(
             this Microsoft.Azure.Cosmos.Table.CloudTable table,
             Microsoft.Azure.Cosmos.Table.TableQuery<TInput> query,
             int pageSize,
             Func<TInput, Task<TOutput>> conversion,
             CosmosTableBasedContinuationToken continuationToken,
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

        public static async Task<PagedListResult<TOutput>> PaginateListResultAsync<TInput, TOutput>(
            this CloudTable table,
            TableQuery<TInput> query,
            int pageSize,
            Func<TInput, Task<TOutput>> conversion,
            CloudTableBasedContinuationToken continuationToken,
            CancellationToken cancelationToken = default(CancellationToken))
            where TInput : ITableEntity, new()
        {
            var items = new List<TOutput>();
            TableContinuationToken token = continuationToken.IsContinuation() ? continuationToken.TableCursor : null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            query = query.Take(pageSize);

            var seg = await table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
            continuationToken.TableCursor = seg.ContinuationToken;
            items.AddRange(
                (await Task.WhenAll(seg.Select(async x => await conversion(x))))
                .Where(x => x != null)
            );

            return new PagedListResult<TOutput> { Items = items, ContinuationToken = continuationToken?.IntoToken() };
        }

        [Obsolete("Use PaginateListResult with pageSize")]
        public static async Task<PagedListResult<TOutput>> PaginateListResult<TInput, TOutput>(
            this CloudTable table,
            TableQuery<TInput> query,
            Func<TInput, TOutput> conversion,
            CloudTableBasedContinuationToken continuationToken,
            CancellationToken cancelationToken = default(CancellationToken))
            where TInput : ITableEntity, new()
        {
            var items = new List<TOutput>();
            TableContinuationToken token = continuationToken.IsContinuation() ? continuationToken.TableCursor : null;
            var oc = new OperationContext();
            var ro = new TableRequestOptions();

            var seg = await table.ExecuteQuerySegmentedAsync(query, token, ro, oc, cancelationToken);
            continuationToken.TableCursor = seg.ContinuationToken;
            items.AddRange(
                seg.Select(x => conversion(x))
                .Where(x => x != null)
            );

            return new PagedListResult<TOutput> { Items = items, ContinuationToken = continuationToken?.IntoToken() };
        }
    }
}
