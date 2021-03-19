using Crey.Extensions;
using Crey.Misc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    // combinators for TaskLike<ContainerLike<T>> (hopefully in C# 10 will not need to do such repeats)
    // given {Task | ValueTask} x {IEnumerable | IList | List | [] | HashSet | ... } x Tuple 1,2,3,4 there would be several hundreads of such methods
    public static class AsyncLinqExtensions
    {
        /// <summary>
        /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value
        /// </summary> 
        [DebuggerStepThrough]
        public static async Task<T> TapAsync<T>(this Task<T> x, Action<T> effect)
        {
            var r = await x;
            effect(r);
            return r;
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, T3, TOut>(this (Task<T1> t1, Task<T2> t2) self, T3 c, Func<T1, T2, T3, Task<TOut>> func)
        {
            var t1 = await self.t1;
            var t2 = await self.t2;
            var t3 = c;
            var result = func(t1, t2, t3);
            return await result;
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, T3, TOut>(this (Task<T1> t1, Task<T2> t2, T3 t3) self, Func<T1, T2, T3, Task<TOut>> func)
        {
            var t1 = await self.t1;
            var t2 = await self.t2;
            var t3 = self.t3;
            var result = func(t1, t2, t3);
            return await result;
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, TOut>(this Task<T1> self, T2 t2, Func<T1, T2, Task<TOut>> func)
        {
            var t1 = await self;
            var result = func(t1, t2);
            return await result;
        }

        // https://github.com/dotnet/csharplang/issues/1349
        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<TIn, TOut>(this Task<TIn> self)
            where TIn : ITo<TOut>
        {
            var t1 = await self;
            var result = t1.To<TOut>();
            return result;
        }

        [DebuggerStepThrough]
        public async static Task<T> SingleAsync<T>(this Task<IEnumerable<T>> self)
        {
            var t1 = await self;
            return t1.Single();
        }

        [DebuggerStepThrough]
        public async static Task<T> SingleAsync<T>(this Task<IEnumerable<T>> self, Func<T, bool> filter)
        {
            var t1 = await self;
            return t1.Single(filter);
        }

        [DebuggerStepThrough]
        public async static Task<T> SingleAsync<T>(this Task<List<T>> self, Func<T, bool> filter)
        {
            var t1 = await self;
            return t1.Single(filter);
        }

        [DebuggerStepThrough]
        public async static Task<T> SingleAsync<T>(this Task<List<T>> self)
        {
            var t1 = await self;
            return t1.Single();
        }

        [DebuggerStepThrough]
        public async static Task ToAsync<T1, T2>(this Task<T1> self, T2 t2, Func<T1, T2, Task> func)
        {
            var t1 = await self;
            await func(t1, t2);
        }

        [DebuggerStepThrough]
        public async static Task<T> SingleAsync<T>(this Task<T[]> self)
        {
            var t1 = await self;
            return t1.Single();
        }

        [DebuggerStepThrough]
        public async static Task<T> SingleAsync<T>(this Task<IList<T>> self)
        {
            var t1 = await self;
            return t1.Single();
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, T3, TOut>(this (T1 t1, Task<T2> t2) self, T3 c, Func<T1, T2, T3, Task<TOut>> func)
        {
            var t1 = self.t1;
            var t2 = await self.t2;
            var t3 = c;
            var result = func(t1, t2, t3);
            return await result;
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, T3, TOut>(this (T1 t1, Task<T2> t2, T3 t3) self, Func<T1, T2, T3, Task<TOut>> func)
        {
            var t1 = self.t1;
            var t2 = await self.t2;
            var t3 = self.t3;
            var result = func(t1, t2, t3);
            return await result;
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, T3, TOut>(this (Task<T1> t1, T2 t2) self, T3 c, Func<T1, T2, T3, Task<TOut>> func)
        {
            var t1 = await self.t1;
            var t2 = self.t2;
            var t3 = c;
            var result = func(t1, t2, t3);
            return await result;
        }

        [Obsolete("Wrong name. Do not use. Use ToAsync")]
        [DebuggerStepThrough]
        public static async Task<TResult> SelectAsync<T, TResult>(this Task<T> self, Func<T, TResult> map) =>
            map(await self.IgnoreContext());

        [DebuggerStepThrough]
        public static async Task<TResult> ToAsync<T, TResult>(this Task<T> self, Func<T, TResult> map) =>
            map(await self.IgnoreContext());

        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> WhereAsync<T>(this Task<List<T>> self, Func<T, bool> filter)
        {
            var data = await self;
            return data.Where(filter);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> WhereAsync<T>(this Task<IList<T>> self, Func<T, bool> filter)
        {
            var data = await self;
            return data.Where(filter);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> WhereAsync<T>(this Task<IEnumerable<T>> self, Func<T, bool> filter)
        {
            var data = await self;
            return data.Where(filter);
        }

        [DebuggerStepThrough]
        public static async Task<TResult> ToAsync<T, TResult>(this Task<T> self, Func<T, Task<TResult>> map) =>
            await map(await self.IgnoreContext()).IgnoreContext();

        [DebuggerStepThrough]
        public static async Task<TResult> To<T, TResult>(this Task<T> self, Func<T, TResult> map)
        {
            var result = await self;
            return map(result);
        }

        [Obsolete("Do not use. Wrong name. Use to async.")]
        [DebuggerStepThrough]
        public static async ValueTask<TResult> SelectAsync<T, TResult>(this ValueTask<T> x, Func<T, TResult> map) =>
                    map(await x.IgnoreContext());

        /// <summary>
        /// Returns completed task if current is null.
        /// </summary>
        public static Task IfNullCompleted(this Task self) => self ?? Task.CompletedTask;

        [DebuggerStepThrough]
        public static async ValueTask<TResult> ToAsync<T, TResult>(this ValueTask<T> x, Func<T, Task<TResult>> map) =>
            await map(await x.IgnoreContext()).IgnoreContext();

        [DebuggerStepThrough]
        public static async Task ToAsync<T>(this Task<T> x, Func<T, Task> map) =>
            await map(await x.IgnoreContext()).IgnoreContext();

        [Obsolete]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> IgnoreContext<T>(this Task<T> task) =>
            task.ConfigureAwait(false);

        [Obsolete]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable IgnoreContext(this Task task) =>
            task.ConfigureAwait(false);

        [Obsolete]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskAwaitable<T> IgnoreContext<T>(this ValueTask<T> task) =>
            task.ConfigureAwait(false);

        [Obsolete]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskAwaitable IgnoreContext(this ValueTask task) =>
            task.ConfigureAwait(false);

        // we do not have type classes, so just produce combination for each container(List/Enumerable or Task/Value task for our cases)
        // once written -  gets reused
        public static async Task<IEnumerable<TResult>> SelectAsync<TOut, TResult>(this Task<List<TOut>> self, Func<TOut, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        public static async Task<IEnumerable<TResult>> SelectAsync<TKey, TValue, TResult>(this Task<IDictionary<TKey, TValue>> self, Func<KeyValuePair<TKey, TValue>, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        [DebuggerStepThrough]
        public static Task<IEnumerable<TResult>> SelectToAsync<T1, T2, TResult>(this Task<IEnumerable<T2>> self, T1 first, Func<T1, T2, TResult> selector) =>
            self.SelectAsync(_ => _.To(first, selector));

        [DebuggerStepThrough]
        public static Task<IEnumerable<TResult>> SelectToAsync<T1, T2, TResult>(this Task<List<T2>> self, T1 first, Func<T1, T2, TResult> selector) =>
            self.SelectAsync(_ => _.To(first, selector));

        [DebuggerStepThrough]
        public static Task<IEnumerable<TResult>> SelectToAsync<T1, T2, T3, TResult>(this Task<List<T3>> self, T1 first, T2 second, Func<T1, T2, T3, TResult> selector) =>
            self.SelectAsync(_ => _.To(first, second, selector));

        [DebuggerStepThrough]
        public static Task<IEnumerable<TResult>> SelectToAsync<T1, T2, T3, TResult>(this Task<IEnumerable<T3>> self, T1 first, T2 second, Func<T1, T2, T3, TResult> selector) =>
            self.SelectAsync(_ => _.To(first, second, selector));

        [DebuggerStepThrough]
        public static async Task<IEnumerable<TResult>> SelectAsync<TOut, TResult>(this Task<HashSet<TOut>> self, Func<TOut, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<TResult>> SelectAsync<TOut, TResult>(this Task<TOut[]> self, Func<TOut, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<TResult>> SelectAsync<TOut, TResult>(this Task<IList<TOut>> self, Func<TOut, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<TResult>> SelectAsync<TOut, TResult>(this Task<IEnumerable<TOut>> self, Func<TOut, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        [DebuggerStepThrough]
        public static async ValueTask<IEnumerable<TResult>> SelectAsync<TOut, TResult>(this ValueTask<IEnumerable<TOut>> self, Func<TOut, TResult> selector)
        {
            var items = await self;
            return items.Select(selector);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<Task<T>> self, Func<T, IEnumerable<TResult>> map)
        {
            var results = new List<TResult>();
            foreach (var item in self)
            {
                var result = await item;
                results.AddRange(map(result));
            }

            return results;
        }

        [DebuggerStepThrough]
        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(this Task<IEnumerable<T>> self, Func<T, TKey> key, Func<T, TValue> value)
        {
            var result = await self;
            return result.ToDictionary(key, value);
        }

        [DebuggerStepThrough]
        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this Task<IEnumerable<KeyValuePair<TKey, TValue>>> self)
        {
            var result = await self;
            return result.ToDictionary(x => x.Key, x => x.Value);
        }

        [DebuggerStepThrough]
        public static async ValueTask<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<ValueTask<T>> self, Func<T, IEnumerable<TResult>> map)
        {
            var results = new List<TResult>();
            foreach (var item in self)
            {
                var result = await item;
                results.AddRange(map(result));
            }

            return results;
        }

        [DebuggerStepThrough]
        public static async ValueTask<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(this ValueTask<IEnumerable<T>> self, Func<T, TKey> key, Func<T, TValue> value)
        {
            var result = await self;
            return result.ToDictionary(key, value);
        }

        [DebuggerStepThrough]
        public static async Task<HashSet<T>> ToHashSetAsync<T>(this Task<IEnumerable<T>> self)
        {
            var result = await self;
            return result.ToHashSet();
        }

        [DebuggerStepThrough]
        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> self)
        {
            var result = await self;
            return result.ToList();
        }

        [DebuggerStepThrough]
        public static async Task<T> FirstOrDefaultAsync<T>(this Task<List<T>> self)
        {
            var data = await self;
            return data.FirstOrDefault();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IEnumerable<T>> self)
        {
            var data = await self;
            return data.FirstOrDefault();
        }

        [DebuggerStepThrough]
        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IEnumerable<T>> self, Func<T, bool> predicate)
        {
            var data = await self;
            return data.FirstOrDefault(predicate);
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> ShuffleAsync<T>(this Task<List<T>> self)
        {
            var data = await self;
            return data.Shuffle();
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> ShuffleAsync<T>(this Task<IEnumerable<T>> self)
        {
            var data = await self;
            return data.Shuffle();
        }

        [DebuggerStepThrough]
        public static async Task<T> TapAsync<T>(this Task<T> x, Func<T, Task> effect)
        {
            var r = await x;
            await effect(r);
            return r;
        }

        [DebuggerStepThrough]
        public static async Task<T> TapAsync<T>(this Task<T> x, Func<Task> effect)
        {
            var r = await x;
            await effect();
            return r;
        }

        [DebuggerStepThrough]
        public static async Task WhenAllAsync(this Task<IEnumerable<Task>> self)
        {
            var data = await self;
            await Task.WhenAll(data);
        }

        [DebuggerStepThrough]
        public static async Task<T> ReduceOrDefaultAsync<T>(this Task<IEnumerable<T>> self, Func<IEnumerable<T>, T> reducer)
        {
            var data = await self;
            return !data.Any() ? default : reducer(data);
        }

        [DebuggerStepThrough]
        public static async Task<T> ReduceOrDefaultAsync<T1, T>(this Task<IEnumerable<T>> self, T1 a, Func<T1, IEnumerable<T>, T> reducer)
        {
            var data = await self;
            return !data.Any() ? default : reducer(a, data);
        }

        [DebuggerStepThrough]
        public static async Task<T> ReduceOrDefaultAsync<T1, T>(this Task<List<T>> self, T1 a, Func<T1, List<T>, T> reducer)
        {
            var data = await self;
            return !data.Any() ? default : reducer(a, data);
        }

        [DebuggerStepThrough]
        public static HashSet<TKey> ToHashSet<TSource, TKey>(this IEnumerable<TSource> self, Func<TSource, TKey> toKey)
        {
            return self.Select(toKey).ToHashSet();
        }

        [DebuggerStepThrough]
        public async static Task<TOut> ToAsync<T1, T2, T3, TOut>(this Task<(T1, Task<T2>)> self, T3 t3, Func<T1, T2, T3, Task<TOut>> func)
        {
            var (t1, t2) = await self;
            var t2Value = await t2;
            var result = func(t1, t2Value, t3);
            return await result;
        }

        [DebuggerStepThrough]
        public static async Task<IEnumerable<TOut>> CastAsync<TIn, TOut>(this Task<IEnumerable<TIn>> self)
        {
            var data = await self;
            return data.Cast<TOut>();
        }

    }
}
