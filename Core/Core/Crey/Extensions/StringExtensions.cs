using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crey.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveSpecialCharacters(this string input)
        {
            return Regex.Replace(input, "(?:[^a-z0-9 ]|(?<=['\"])s)", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        }

        public static string PrettifyJsonToHtml(this string ugly)
        {
            return ugly
                .Replace(",\"", ",<br>&emsp;\"<b>") // from second key
                .Replace("{\"", "{<br>&emsp;\"<b>") // first key
                .Replace("\":", "</b>\":") // key end
                .Replace("}", "<br>}");
        }

        public static byte[] ToUtf8Bytes(this string source)
        {
            return Encoding.UTF8.GetBytes(source);
        }

        public static bool IsVoid(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// The jsonValue must be a properly formatted / escaped json string like 
        /// ["a","b"] or [{..},{..}] array 
        /// \"eight\" string must contain the quotes
        /// 213423 or { ... }
        /// </summary>
        /// <param name="jsonValue"></param>
        /// <param name="keyName"></param>
        /// <param name="asArray"></param>
        /// <returns></returns>
        public static string ToJsonWrap(this string jsonValue, string keyName, bool asArray = true)
        {
            if (asArray)
            {
                if (jsonValue.IsVoid()) jsonValue = "[]";
            }
            else
            {
                if (jsonValue.IsVoid()) throw new Exception("Ambiguous usage of jsonValue");
            }

            return $"{{\"{keyName}\":{jsonValue}}}";
        }


        public static string GenereateRelayKey(string stage, string machineName, int port)
        {
            return $"relay_{stage}_{machineName}_{port}";
        }

        public static bool ContainsAnyCharacter(this string str, char[] array)
        {
            return str.IndexOfAny(array) != -1;
        }

        public static bool ContainsOnlyCharacter(this string str, char[] allowed)
        {
            foreach (char c in str)
            {
                if (!allowed.Contains(c))
                    return false;
            }

            return true;
        }

        public static string Capitalize(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}

