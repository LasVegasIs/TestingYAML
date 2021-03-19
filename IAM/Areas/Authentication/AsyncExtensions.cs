using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public static class AsyncExtensions
    {
        public static async Task<T> ThrowIfNullAsync<T>(this Task<T> source, Func<Exception> exceptionFactory)
        {
            var data = await source;
            if (Equals(data, null))
                throw exceptionFactory();

            return data;
        }
    }
}
