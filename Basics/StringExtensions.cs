namespace SpaceEngineers.Core.Basics
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// System.String extensions
    /// </summary>
    public static class StringExtensions
    {
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