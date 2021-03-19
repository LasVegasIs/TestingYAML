using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Crey.Extensions
{
    public static class EnumerableExtensions
    {


        public static IEnumerable<T> ConcatSingle<T>(this IEnumerable<T> enumerable, T value)
        {
            return enumerable.Concat(new[] { value });
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null)
                return true;

            var e = enumerable.GetEnumerator();
            return !e.MoveNext();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>
        ///  {1,2,3,4} => "1,2,3,4"
        /// </summary>
        public static string ArrayToString<T>(this IEnumerable<T> enumerable, string separator = ",")
        {
            var array = enumerable as T[] ?? enumerable.ToArray();
            return array.IsNullOrEmpty() ? string.Empty : array.Select(a => a.ToString()).Aggregate((a, c) => a + separator + c);
        }

        public static List<string> SplitToList(this string text, char delimiter = ',')
        {
            return text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToList();

        }

        public static T PickRandomElement<T>(this IEnumerable<T> source)
        {
            return source.PickRandomElements(1).Single();
        }

        public static IEnumerable<T> PickRandomElements<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}