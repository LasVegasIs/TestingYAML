using System.Collections.Generic;

namespace Core.Extensions
{
    public static class SetOperations
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> collection)
        {
            return new HashSet<T>(collection);
        }

        public static HashSet<T> Subtract<T>(this HashSet<T> set, IEnumerable<T> other)
        {
            var clone = set.ToSet();
            clone.ExceptWith(other);
            return clone;
        }
    }
}
