#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Crey.Data.Sql
{
    public enum TransactionResultType
    {
        // in action
        Done,
        DoneWithoutCommit,

        //in conflicts
        RetryWithoutRollback,
        Retry,
        ResolveAsDone,
        Fail,
    }

    public class TransactionResult
    {
        public static TransactionResult<T> Done<T>(T result)
        {
            return new TransactionResult<T> { State = TransactionResultType.Done, Result = result };
        }

        public static TransactionResult<T> DoneWithoutCommit<T>(T result) where T : class
        {
            return new TransactionResult<T> { State = TransactionResultType.DoneWithoutCommit, Result = result };
        }

        public static TransactionResult<T> Retry<T>()
        {
            return new TransactionResult<T> { State = TransactionResultType.Retry };
        }
        public static TransactionResult<T> RetryWithoutRollback<T>() where T : class
        {
            return new TransactionResult<T> { State = TransactionResultType.RetryWithoutRollback };
        }

        public static TransactionResult<T> Fail<T>() where T : class
        {
            return new TransactionResult<T> { State = TransactionResultType.Fail };
        }

        public static TransactionResult<T> ResolveAsDone<T>(T result) where T : class
        {
            return new TransactionResult<T> { State = TransactionResultType.ResolveAsDone, Result = result };
        }
    }

    public class TransactionResult<T>
    {
        public TransactionResultType State { get; set; }
        public T? Result { get; set; }
    }

    public static class BulkDBContext
    {
        public static async Task<T?> RetryTransactionFuncAsync<T, D>(this D dbContext,
            int concurrencyRetryCount,
            Func<D, IDbContextTransaction, Task<TransactionResult<T>>> action,
            Func<D, IDbContextTransaction, IReadOnlyList<EntityEntry>, Task<TransactionResult<T>>> onConflict,
            Func<D, Task<T>> onFailure)
            where D : DbContext
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                for (int retry = concurrencyRetryCount; retry > 0; retry--)
                {
                    using (IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var res = await action(dbContext, transaction);
                            switch (res.State)
                            {
                                case TransactionResultType.Done: await transaction.CommitAsync(); return res.Result;
                                case TransactionResultType.DoneWithoutCommit: return res.Result;
                                default: throw new Exception($"invalid state for action: {res.State}");
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var res = await onConflict(dbContext, transaction, ex.Entries);
                            switch (res.State)
                            {
                                case TransactionResultType.Retry: await transaction.RollbackAsync(); dbContext.BulkResetDbSets(); break;
                                case TransactionResultType.RetryWithoutRollback: break;
                                case TransactionResultType.Fail: retry = 0; break;
                                case TransactionResultType.ResolveAsDone: return res.Result;
                                default: throw new Exception($"invalid state for conflict: {res.State}");
                            }
                        }
                    }
                }
                return await onFailure(dbContext);
            });
        }

        public static IEnumerable<PropertyInfo> GetDbSetProperties(this DbContext context)
        {
            var properties = context.GetType().GetProperties();

            foreach (var property in properties)
            {
                var setType = property.PropertyType;
                var isDbSet = setType.IsGenericType && (typeof(DbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()));
                if (isDbSet)
                {
                    yield return property;
                }
            }
        }

        public static void BulkForgetChanges(this DbContext context)
        {
            var list = context.ChangeTracker.Entries().ToList(); // store as foreach would alter the enumerator
            foreach (var entry in list)
                entry.State = EntityState.Detached;
        }

        [Obsolete("EF Core 5 has DB Context Factory + Clear change tracker method for this")]
        public static void BulkResetDbSets(this DbContext context)
        {
            context.BulkForgetChanges();

            foreach (var p in context.GetDbSetProperties())
            {
                var db = p.GetValue(context)!;
                var localProp = db.GetType().GetProperty("Local");
                var local = localProp!.GetValue(db);
                var clear = local!.GetType().GetMethod("Clear");
                clear!.Invoke(local, null);
            }
        }
    }
}
