using System;
using System.Threading.Tasks;

namespace Crey.Extensions
{
    public static class TaskUtilities
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // do nothing, because we cannot inject anything here that would not be disposed before we try to use it
            }
        }
    }
}