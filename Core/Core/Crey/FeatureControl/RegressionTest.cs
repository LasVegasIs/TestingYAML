using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Crey.FeatureControl
{
    public class RegressionTest
    {
        [Conditional("DEBUG")]
        public static void Break()
        {
            Debugger.Break();
        }

        private readonly ILogger logger_;

        public RegressionTest(ILogger<RegressionTest> logger)
        {
            logger_ = logger;
        }

        public void DeprecatedUsage(
            string extraInfo,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLine = -1
        )
        {
            logger_.LogCritical("Regression {} {}:{} - deprecated usage: {}", memberName, sourceFilePath, sourceLine, extraInfo);
            Break();
        }

        public T CheckRegression<T>(
            Func<T> oldMethod,
            Func<T> newMethod,
            Func<T, T, string> validate,
            string extraInfo,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLine = -1
        )
        {
            logger_.LogTrace("Regression {} {}:{} - old method", memberName, sourceFilePath, sourceLine);
            var oldStart = DateTime.UtcNow;
            var oldResult = oldMethod();
            var oldTime = DateTime.UtcNow - oldStart;
            logger_.LogTrace("Regression {} {}:{} - old method done, {}", memberName, sourceFilePath, sourceLine, oldTime);

            try
            {
                logger_.LogTrace("Regression {} {}:{} - new method", memberName, sourceFilePath, sourceLine);
                var newStart = DateTime.UtcNow;
                var newResult = newMethod();
                var newTime = DateTime.UtcNow - newStart;
                logger_.LogTrace("Regression {} {}:{} - new method done, {}", memberName, sourceFilePath, sourceLine, newTime);

                logger_.LogDebug("Regression {} {}:{}, time change: {} -> {}", memberName, sourceFilePath, sourceLine, oldTime, newTime);

                var va = validate(oldResult, newResult);
                if (va != null)
                {
                    logger_.LogCritical("Regression {} {}:{}, validation failed: {}. extraInfo: {}", memberName, sourceFilePath, sourceLine, va, extraInfo);
                    Break();
                }
            }
            catch (Exception ex)
            {
                logger_.LogCritical("Regression {} {}:{}, Exception: {}. extraInfo: {}", memberName, sourceFilePath, sourceLine, ex, extraInfo);
                Break();
            }

            return oldResult;
        }
    }
}
