namespace SpaceEngineers.Core.Basics
{
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
        public static bool IsNullOrEmpty(this string? source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}