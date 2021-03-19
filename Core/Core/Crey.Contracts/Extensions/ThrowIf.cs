using Core.Extensions.ThrowIf;
using Crey.Contracts;

//namespace renamed to catch if anyone is using it
namespace Crey.Extensions_deprecated.ThrowIf
{
    public static class ThrowIfExtension
    {
        public static void ThrowIfTrue(this bool value, ErrorCodes error, string message = "")
        {
            value.ThrowIfTrue(() => error.IntoException(message));
        }

        public static void ThrowIfFalse(this bool value, ErrorCodes error, string message = "")
        {
            value.ThrowIfFalse(() => error.IntoException(message));
        }

        public static T ThrowIfNull<T>(this T source, ErrorCodes error, string message = "")
        {
            return source.ThrowIfNull(() => error.IntoException(message));
        }

        public static T ThrowIfNotNull<T>(this T source, ErrorCodes error, string message = "")
        {
            return source.ThrowIfNotNull(() => error.IntoException(message));
        }

        public static int ThrowIfZero(this int source, ErrorCodes error, string message = "")
        {
            return source.ThrowIfZero(() => error.IntoException(message));
        }

        public static long ThrowIfZero(this long source, ErrorCodes error, string message = "")
        {
            return source.ThrowIfZero(() => error.IntoException(message));
        }

        public static T ThrowIfEqual<T>(this T source, T comparer, ErrorCodes error, string message = "")
        {
            return source.ThrowIfEqual(comparer, () => error.IntoException(message));
        }

        public static T ThrowIfNotEqual<T>(this T source, T comparer, ErrorCodes error, string message = "")
        {
            return source.ThrowIfNotEqual(comparer, () => error.IntoException(message));
        }

        public static T ThrowIfType<T>(this T source, ErrorCodes error, string message = "")
        {
            return source.ThrowIfType<T>(() => error.IntoException(message));
        }

        public static T ThrowIfNotType<T>(this object source, ErrorCodes error, string message = "")
        {
            return source.ThrowIfNotType<T>(() => error.IntoException(message));
        }
    }
}
