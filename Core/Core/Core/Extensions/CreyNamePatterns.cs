using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Extension.CreyNamePatterns
{
    public static class CreyNamePatterns
    {
        public static string RemoveSpecialCharacters(this string input)
        {
            return Regex.Replace(input, "(?:[^a-z0-9 ]|(?<=['\"])s)", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        }

        public static string SubstituteCreyStagePattern(this string pattern, string inputStage, string inputService, string inputName)
        {
            var stage = inputStage.ToLower();
            var capStage = string.IsNullOrEmpty(stage) ? stage : stage.First().ToString().ToUpper() + stage.Substring(1);
            var negStage = string.IsNullOrEmpty(stage) ? "" : "-" + stage;
            var service = inputService.ToLower();
            var capService = string.IsNullOrEmpty(service) ? service : service.First().ToString().ToUpper() + service.Substring(1);
            var negService = string.IsNullOrEmpty(service) ? "" : "-" + service;

            var name = inputName.ToLower();
            var capName = string.IsNullOrEmpty(name) ? name : name.First().ToString().ToUpper() + name.Substring(1);
            var negName = string.IsNullOrEmpty(name) ? "" : "-" + name;

            return pattern
                .Replace("${stage}", stage)
                .Replace("${Stage}", capStage)
                .Replace("${-stage}", negStage)
                .Replace("${service}", service)
                .Replace("${Service}", capService)
                .Replace("${-service}", negService)
                .Replace("${name}", name)
                .Replace("${Name}", capName)
                .Replace("${-name}", negName);
        }
    }
}
