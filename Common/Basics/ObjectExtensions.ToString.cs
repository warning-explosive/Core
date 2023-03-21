namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections;
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
        /// <returns>Property values of passed instance</returns>
        public static string Dump(this object instance, BindingFlags flags)
        {
            return DumpValue(instance, flags, 0, new HashSet<object>()).ToString(Environment.NewLine);

            static IEnumerable<string> DumpValue(
                object? value,
                BindingFlags flags,
                int depth,
                HashSet<object> visited)
            {
                if (value != null && value.GetType().IsCollection())
                {
                    var enumerator = ((IEnumerable)value).GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        foreach (var str in DumpValue(enumerator.Current, flags, depth, visited))
                        {
                            yield return str;
                        }
                    }
                }
                else if (value != null && !value.GetType().IsPrimitive() && visited.Add(value))
                {
                    var properties = value.GetType().GetProperties(flags);

                    foreach (var property in properties)
                    {
                        yield return $"{new string('\t', depth)}{property.Name}";

                        foreach (var str in DumpValue(property.GetValue(value), flags, depth + 1, visited))
                        {
                            yield return str;
                        }
                    }

                    visited.Remove(value);
                }
                else
                {
                    yield return $"{new string('\t', depth)}{value?.ToString() ?? "null"}";
                }
            }
        }
    }
}