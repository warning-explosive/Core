namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// System.String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Starts source string from capital letter
        /// </summary>
        /// <param name="source">Source string</param>
        /// <returns>Source string started from capital letter</returns>
        public static string StartFromCapitalLetter(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            return string.Create(
                source.Length,
                source,
                static (buffer, source) =>
                {
                    buffer[0] = char.ToUpper(source[0], CultureInfo.InvariantCulture);
                    source.AsSpan(1).ToLowerInvariant(buffer.Slice(1));
                });
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

        /// <summary>
        /// Formats string with invariant culture
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Args</param>
        /// <returns>Format result</returns>
        public static string Format(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Formats string
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="formatProvider">Format provider</param>
        /// <param name="args">Args</param>
        /// <returns>Format result</returns>
        public static string Format(this string format, IFormatProvider formatProvider, params object[] args)
        {
            return string.Format(formatProvider, format, args);
        }
    }
}