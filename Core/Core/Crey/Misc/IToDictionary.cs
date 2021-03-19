using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace Crey.Misc
{
    /// allows to write diciplined casts (which do not fail) and make these async out of box
    public interface ITo<TTo>
    {
        T To<T>() where T : TTo;
    }

    /// <summary>
    /// Converts simple data object into key values. Where keys are property names and values are <see cref="Object.ToString()"/>.
    /// </summary>
    public interface IToDictionary
    {

        /// <summary>
        /// Converts  this to dictionary of strings.
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead).ToArray();
            var dict = new Dictionary<string, string>();
            foreach (var prop in props)
                dict.Add(prop.Name, $"{prop.GetValue(this)}");
            return dict;
        }
    }

    // static cache pattern (fastest possible)
    internal static class Cache<T>
    {
        internal static PropertyInfo[] props_;

        // TODO: compile expression to read all out as dictionary if not AOT
        static Cache() =>
            props_ = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead).ToArray();
    }

    /// <inheritdoc/>
    public interface IToDictionary<T> : IToDictionary
    {
        /// <inheritdoc/>
        public new Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in Cache<T>.props_)
            {
                if (prop.PropertyType.Name.Contains("IToDictionary"))
                {
                    // TODO: merge dictionary into dot prefixed names
                    // TODO: hadle dictionary attributed of system text json
                }
                dict.Add(prop.Name, $"{prop.GetValue(this)}");
            }

            return dict;
        }
    }
}