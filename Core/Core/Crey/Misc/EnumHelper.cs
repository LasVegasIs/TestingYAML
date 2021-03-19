using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Crey.Misc
{
    public static class EnumHelper
    {
        /// <summary>
        /// Converts an enum to a dictionary
        /// </summary>
        public static Dictionary<string, object> ToDictionary<T>()
        {
            var result = new Dictionary<string, object>();

            var values = Enum.GetValues(typeof(T));

            for (var i = 0; i < values.Length; i++)
            {
                var value = values.GetValue(i);
                var name = Enum.GetName(typeof(T), value);
                Debug.Assert(name != null, "name != null");
                result[name] = Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)));
            }

            return result;
        }

        public static int MaximumValue<T>()
        {
            var maxValue = 0;
            var values = Enum.GetValues(typeof(T));

            for (var i = 0; i < values.Length; i++)
            {
                var value = values.GetValue(i);
                var tVal = Convert.ToInt32(value);
                if (tVal > maxValue) maxValue = tVal;
            }

            return maxValue;
        }

        /// <summary>
        /// Returns the max enum name string length
        /// </summary>
        public static int MaxNameLength<T>()
        {
            return Enum.GetNames(typeof(T)).Select(n => n.Length).Max();
        }


        public static IEnumerable<(string name, T value)> GetEnumPair<T>(this Type enumType) where T : struct
        {
            if (!enumType.IsEnum) throw new Exception("Wtf? Type is not enum");
            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType).Cast<T>().ToArray();

            for (var i = 0; i < names.Length; i++)
            {
                yield return (names[i], values[i]);
            }
        }
    }
}