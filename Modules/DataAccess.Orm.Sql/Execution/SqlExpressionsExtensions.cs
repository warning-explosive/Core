namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Execution
{
    using System;

    /// <summary>
    /// SqlExpressionsExtensions
    /// </summary>
    public static class SqlExpressionsExtensions
    {
        private const char Percent = '%';

        /// <summary>
        /// Like
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="pattern">Pattern</param>
        /// <returns>Does the source contain entry with specified pattern or not</returns>
        public static bool Like(this string source, string pattern)
        {
            var starts = pattern.StartsWith(Percent);
            var ends = pattern.EndsWith(Percent);
            var cleanPattern = pattern.Trim(Percent);

            if (starts && ends)
            {
                return source.Contains(cleanPattern, StringComparison.Ordinal);
            }
            else if (!starts && ends)
            {
                return source.StartsWith(cleanPattern, StringComparison.Ordinal);
            }
            else if (starts && !ends)
            {
                return source.EndsWith(cleanPattern, StringComparison.Ordinal);
            }
            else
            {
                return source.Equals(pattern, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// IsNull
        /// </summary>
        /// <param name="source">Source string</param>
        /// <returns>Does the value represent null</returns>
        public static bool IsNull(this object? source)
        {
            return source is null;
        }

        /// <summary>
        /// IsNull
        /// </summary>
        /// <param name="source">Source string</param>
        /// <returns>Does the value represent null</returns>
        public static bool IsNotNull(this object? source)
        {
            return source is not null;
        }
    }
}