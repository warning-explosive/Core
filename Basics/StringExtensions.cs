namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// System.String extensions
    /// </summary>
    public static class StringExtensions
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
        /// IsNullOrEmpty
        /// </summary>
        /// <param name="source">Source string</param>
        /// <returns>IsNullOrEmpty attribute</returns>
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}