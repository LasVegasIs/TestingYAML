using MoreLinq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static Func<T2, TResult> Partial<T1, T2, TResult>(T1 first, Func<T1, T2, TResult> map)
        {
            Func<T2, TResult> func = t2 => map(first, t2);
            return func;
        }

        /// <summary>
        /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value
        /// </summary> 
        public static T Tap<T>(this T x, Action<T> effect)
        {
            effect(x);
            return x;
        }

        /// <summary>
        /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value and the state object
        /// </summary> 
        public static T Tap<T, S>(this T x, S state, Action<T, S> effect)
        {
            effect(x, state);
            return x;
        }

        public static IEnumerable<TResult> SelectTo<T1, T2, TResult>(this IEnumerable<T2> self, T1 first, Func<T1, T2, TResult> selector) =>
            self.Select(_ => _.To(first, selector));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static IEnumerable<TResult> SelectTo<T1, T2, T3, TResult>(this IEnumerable<T3> self, T1 first, T2 second, Func<T1, T2, T3, TResult> selector) =>
                self.Select(_ => _.To(first, second, selector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult To<T1, T2, T3, TResult>(this (T2 second, T3 third) self, T1 first, Func<T1, T2, T3, TResult> map) =>
            map(first, self.second, self.third);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult To<T1, T2, T3, TResult>(this T3 self, T1 first, T2 second, Func<T1, T2, T3, TResult> map) =>
            map(first, second, self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult To<T1, T2, T3, TResult>(this T3 self, (T1 first, T2 second) part, Func<T1, T2, T3, TResult> map) =>
            map(part.first, part.second, self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult To<T1, T2, T3, TResult>(this T3 self, (T1 first, T2 second) part, Func<(T1, T2), T3, TResult> map) =>
            map(part, self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult To<T1, T2, T3, T4, TResult>(this T4 self, T1 first, T2 second, T3 third, Func<T1, T2, T3, T4, TResult> map) =>
            map(first, second, third, self);

        /// <summary>
        /// Pipe operator for funcitonal chaining.
        /// </summary>
        public static R To<T, R>(this T self, Func<T, R> map) => map(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult To<T1, T2, TResult>(this T2 self, T1 first, Func<T1, T2, TResult> map) =>
            map(first, self);

        // similar to `as..select` in Gremlin or to `let` in C# LINQ
        public static IEnumerable<T> SelectLet<T, V>(this IEnumerable<T> self, out V var, Func<IEnumerable<T>, V> sideEffect)
        {
            var = self.To(sideEffect);
            return self;
        }

        public delegate bool TryFunc<TIn, TOut>(TIn a, out TOut y);

        /// combines Select and Try patters into one liner
        public static IEnumerable<TOut> SelectTry<TIn, TOut>(this IEnumerable<TIn> self, TryFunc<TIn, TOut> tryMap)
        {
            foreach (var item in self)
                if (tryMap(item, out var mapped))
                    yield return mapped;
        }


        // specialize for most common operations
        public delegate bool TryFunc<TOut>(string a, out TOut y);

        public static IEnumerable<TOut> SelectTry<TOut>(this IEnumerable<string> self, TryFunc<TOut> tryMap)
        {
            foreach (var item in self)
                if (tryMap(item, out var mapped))
                    yield return mapped;
        }

        public static IEnumerable<T> SelectLet<T>(this IEnumerable<T> self, out IEnumerable<T> var)
        {
            var = self;
            return self;
        }

        public static void ForEach<T, Ignore>(this IEnumerable<T> self, Func<T, Ignore> action)
        {
            foreach (var x in self)
                action(x);
        }

        public static void ForEach<K, V>(this IDictionary<K, V> self, Action<K, V> action)
        {
            self.AsEnumerable().ForEach(kv => action(kv.Key, kv.Value));
        }

        public static IEnumerable<T> Tap<T>(this IEnumerable<T> self, Action<T> tap)
        {
            var consumed = self.ToArray();
            consumed.ForEach(tap);
            return consumed;
        }

        ///<summary>
        /// If null, than throws.
        /// If not null than passes object as it as not nullable.
        ///</summary>
        public static T NotNull<T>(this T self, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
            where T : class
            =>
            self != null ? self : throw new ArgumentNullException(memberName);


        public static IEnumerable<T1> SelectFirst<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> self) =>
            self.Select(x => x.Item1);
    }
}
