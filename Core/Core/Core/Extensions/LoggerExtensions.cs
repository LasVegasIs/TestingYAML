using Crey.Misc;
using Microsoft.Extensions.Logging;
using System;

namespace Matchmaking.Services
{
    // allows to destucture object into logs (json is not destuctured by providers -> cannot be used)
    public static class LoggerExtensions
    {
        public static void LogError(this ILogger self, EventId eventId, string message, IToDictionary args) => self.LogError(eventId, message, args.ToDictionary());
        public static void LogError<T>(this ILogger self, EventId eventId, string message, IToDictionary<T> args) => self.LogError(eventId, message, args.ToDictionary());
        public static void LogError<T>(this ILogger self, string message, IToDictionary<T> args) => self.LogError(message, args.ToDictionary());

        public static void LogInformation(this ILogger self, string message, IToDictionary args) => self.LogInformation(message, args.ToDictionary());
        public static void LogInformation<T>(this ILogger self, string message, IToDictionary<T> args) => self.LogInformation(message, args.ToDictionary());

        public static IDisposable BeginStructuredScope<T>(this ILogger self, IToDictionary<T> state) => self.BeginScope(state.ToDictionary());
    }
}