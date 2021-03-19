using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    ///             Important: Add new stuff strictly to the end !

    [Obsolete]
    [DataContract]
    public enum ErrorCodes
    {
        [EnumMember] NoError = 0,
        [EnumMember] TimeOut,
        [EnumMember] ServerError,
        [EnumMember] AccountNotFound,
        [EnumMember] ItemNotFound,
        [EnumMember] AccessDenied,
        [EnumMember] InvalidArgument,
        [EnumMember] CommandError,
        [EnumMember] CommunicationError,
    }

    public static class ErrorCodesExtension
    {
        public static CreyException IntoException(this ErrorCodes code, string message = "")
        {
            Debug.Assert(code != ErrorCodes.NoError, $"Throwing NoError as exception");
            return new CreyException(code, message);
        }

        public static void Throw(this ErrorCodes code, string message = "")
        {
            if (code != ErrorCodes.NoError)
                throw new CreyException(code, message);
        }

        public static Error IntoError(this ErrorCodes code, string message = "")
        {
            return new Error(code, message);
        }

        public static Error<T> IntoError<T>(this ErrorCodes code, string message, T detail)
        {
            return new Error<T>(code, message, detail);
        }

        public static Error<T> IntoError<T>(this ErrorCodes code, string message)
            where T : class, new()
        {
            return new Error<T>(code, message, new T());
        }

        public static Error<T> IntoError<T>(this Error error)
            where T : class, new()
        {
            return new Error<T>(error.ErrorCode, error.Message, new T());
        }
    }
}
