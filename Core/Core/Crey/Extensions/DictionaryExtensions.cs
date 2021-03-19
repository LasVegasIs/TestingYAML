using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Safe remove from a concurrent dictionary
        /// </summary>
        public static bool Remove<TK, TV>(this ConcurrentDictionary<TK, TV> d, TK key, out TV value)
        {
            if (d == null)
            {
                value = default(TV);
                return false;
            }

            if (d.TryRemove(key, out value))
                return true;

            var sw = new SpinWait();
            while (d.ContainsKey(key))
            {
                if (d.TryRemove(key, out value))
                    return true;

                sw.SpinOnce();
            }

            value = default(TV);
            return false;
        }

        public static bool Remove<TK, TV>(this ConcurrentDictionary<TK, TV> d, TK key)
        {
            TV value;
            return Remove(d, key, out value);
        }

        public static void RemoveRange<TK, TV>(this IDictionary<TK, TV> source, IEnumerable<TK> collection)
        {
            if (collection == null)
                return;

            using (var e = collection.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    source.Remove(e.Current);
                }
            }
        }

        public static Dictionary<TK, TV2> Map<TK, TV, TV2>(this IDictionary<TK, TV> source, Func<TV, TV2> map)
        {
            return source
                .Select(x => new KeyValuePair<TK, TV2>(x.Key, map(x.Value)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static void AddIfNotNull<T, U>(this IDictionary<T, U> dic, T key, U value)
        {
            if (value != null) { dic.Add(key, value); }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValueProvider();
        }

        public static async Task<TValue> GetValueOrDefaultAsync<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<Task<TValue>> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : await defaultValueProvider();
        }
    }
}