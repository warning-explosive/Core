namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Object ToString extension methods
    /// </summary>
    public static partial class ObjectExtensions
    {
        /// <summary>
        /// Converts enumerable source into string
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="separator">Elements separator</param>
        /// <param name="projection">Projection func</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <returns>String</returns>
        public static string ToString<TSource>(this IEnumerable<TSource> source, string separator, Func<TSource, string>? projection = null)
        {
            projection ??= item => item.ToString();

            return string.Join(separator, source.Select(projection));
        }

        /// <summary>
        /// Show properties of object
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="flags">BindingFlags</param>
        /// <param name="blackList">Black list of properties</param>
        /// <returns>Property values of passed instance</returns>
        public static string ShowProperties(this object instance, BindingFlags flags, params string[] blackList)
        {
            return string.Join(Environment.NewLine,
                        instance.GetType()
                                .GetProperties(flags)
                                .Where(z => !blackList.Contains(z.Name))
                                .Select(z => $"[{z.Name}] - {z.GetValue(instance)?.ToString() ?? "null"}"));
        }
    }
}