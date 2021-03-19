#nullable enable

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crey.Data.Sql
{
    public static class QueryableExtensions
    {
        public static async Task<PagedListResult<TOutput>> PaginateListResultAsync<TInput, TOutput>(
            this IQueryable<TInput> queryable,
            OffsetBasedContinuationToken continuationToken,
            int pageSize,
            Func<TInput, TOutput> conversion)
        {
            int offset = Math.Clamp(continuationToken.Offset, 0, int.MaxValue);
            var data = await queryable.Skip(offset)
                     .Take(pageSize + 1)
                     .ToListAsync();
            var list = data.Select(x => conversion(x))
                     .Where(x => x != null)
                     .ToList();

            OffsetBasedContinuationToken? token = null;
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

        public static async Task<CursorListResult<TOutput>> CursorQueryAsync<TInput, TOutput>(
                this IQueryable<TInput> queryable,
                string? cursor,
                int pageSize,
                Func<TInput, TOutput> conversion)
            where TInput : notnull
            where TOutput : notnull
        {
            int offset = 0;
            if (cursor != null)
            {
                var token = new OffsetBasedContinuationToken(cursor);
                offset = Math.Clamp(token.Offset, 0, int.MaxValue);
            }

            var data = await queryable.Skip(offset)
                     .Take(pageSize + 1)
                     .ToListAsync();
            var list = data
                     .Select(x => conversion(x))
                     .ToList();

            if (list.Count > pageSize)
            {
                var token = new OffsetBasedContinuationToken
                {
                    Offset = offset + pageSize,
                };
                list.RemoveAt(list.Count - 1); // keep it for the next page
                return new CursorListResult<TOutput>(list, token.IntoToken());
            }
            else
            {
                return new CursorListResult<TOutput>(list, null);
            }
        }

        // for caches and in mem collections
        // is must copy pasted because C# does not have HKT
        public static Task<CursorListResult<TOutput>> CursorQueryAsync<TInput, TOutput>(
                this IEnumerable<TInput> self,
                string? cursor,
                int pageSize,
                Func<TInput, TOutput> conversion)
            where TInput : notnull
            where TOutput : notnull
        {
            int offset = 0;
            if (cursor != null)
            {
                var token = new OffsetBasedContinuationToken(cursor);
                offset = Math.Clamp(token.Offset, 0, int.MaxValue);
            }

            var data = self.Skip(offset)
                     .Take(pageSize + 1)
                     .ToList();
            var list = data
                     .Select(x => conversion(x))
                     .ToList();

            if (list.Count > pageSize)
            {
                var token = new OffsetBasedContinuationToken
                {
                    Offset = offset + pageSize,
                };
                list.RemoveAt(list.Count - 1); // keep it for the next page
                return Task.FromResult(new CursorListResult<TOutput>(list, token.IntoToken()));
            }
            else
            {
                return Task.FromResult(new CursorListResult<TOutput>(list, null));
            }
        }
    }
}
