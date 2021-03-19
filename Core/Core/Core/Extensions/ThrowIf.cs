using System;

namespace Core.Extensions.ThrowIf
{
    public static class ThrowIfExtension
    {
        public static void ThrowIfTrue(this bool value, Func<Exception> exceptionFactory)
        {
            if (value)
                throw exceptionFactory();
        }

        public static void ThrowIfFalse(this bool value, Func<Exception> exceptionFactory)
        {
            if (!value)
                throw exceptionFactory();
        }

        public static T ThrowIfNull<T>(this T source, Func<Exception> exceptionFactory)
        {
            if (Equals(source, null))
                throw exceptionFactory();

            return source;
        }

        public static T ThrowIfNotNull<T>(this T source, Func<Exception> exceptionFactory)
        {
            if (!Equals(source, null))
                throw exceptionFactory();

            return source;
        }

        public static int ThrowIfZero(this int source, Func<Exception> exceptionFactory)
        {
            source.ThrowIfEqual(0, exceptionFactory);
            return source;
        }

        public static long ThrowIfZero(this long source, Func<Exception> exceptionFactory)
        {
            source.ThrowIfEqual(0, exceptionFactory);
            return source;
        }

        public static T ThrowIfEqual<T>(this T source, T comparer, Func<Exception> exceptionFactory)
        {
            Equals(source, comparer).ThrowIfTrue(exceptionFactory);
            return source;
        }

        public static T ThrowIfNotEqual<T>(this T source, T comparer, Func<Exception> exceptionFactory)
        {
            Equals(source, comparer).ThrowIfFalse(exceptionFactory);
            return source;
        }

        public static T ThrowIfType<T>(this T actual, Func<Exception> exceptionFactory)
        {
            (actual is T).ThrowIfTrue(exceptionFactory);
            return actual;
        }

        public static T ThrowIfNotType<T>(this object value, Func<Exception> exceptionFactory)
        {
            (value is T).ThrowIfFalse(exceptionFactory);
            return (T)value;
        }
    }
}
