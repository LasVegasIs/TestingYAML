using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crey.Utils
{
    public static class StringExtensions
    {
        public static string FormatWith(this string format, IDictionary<string, object> values)
        {
            var matches = Regex.Matches(format, @"\{(.+?)\}");
            List<string> words = (from Match matche in matches select matche.Groups[1].Value).ToList();

            return words.Aggregate(
                format,
                (current, key) =>
                {
                    int colonIndex = key.IndexOf(':');
                    return current.Replace(
                        "{" + key + "}",
                        colonIndex > 0
                            ? string.Format("{0:" + key.Substring(colonIndex + 1) + "}", values[key.Substring(0, colonIndex)])
                            : values[key].ToString());
                });
        }
    }
}
