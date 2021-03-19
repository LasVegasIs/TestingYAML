using Core.Functional;
using Crey.Contracts;

namespace Crey.Extensions.ErrorIf
{
    public static class ErrorIfExtension
    {
        public static Result<bool, Error> ErrorIfTrue(this bool value, ErrorCodes error, string message = "")
        {
            if (value)
                return error.IntoError(message);
            return value;
        }

        public static Result<bool, Error> ErrorIfFalse(this bool value, ErrorCodes error, string message = "")
        {
            if (!value)
                return error.IntoError(message);
            return value;
        }

        public static Result<T, Error> ErrorIfNull<T>(this T value, ErrorCodes error, string message = "")
        {
            if (value == null)
                return error.IntoError(message);
            return value;
        }

        public static Result<T, Error> ErrorIfNotNull<T>(this T value, ErrorCodes error, string message = "")
        {
            if (value != null)
                return error.IntoError(message);
            return value;
        }

        public static Result<T, Error> IntoResult<T>(this T value)
        {
            return value;
        }
    }
}
