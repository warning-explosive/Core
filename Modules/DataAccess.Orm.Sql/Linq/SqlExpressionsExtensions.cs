namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
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

        /// <summary>
        /// Represents an assign binary operator
        /// </summary>
        /// <param name="left">left operand</param>
        /// <param name="right">right operand</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void Assign<T>(this T left, T right)
        {
            throw new InvalidOperationException($"Method {nameof(Assign)} shouldn't be used outside of expression trees");
        }
    }
}