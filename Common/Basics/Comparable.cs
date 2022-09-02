namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// Comparable
    /// </summary>
    public static class Comparable
    {
        /// <summary>
        /// GreaterOrEquals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static bool Less<T>(T? left, T? right)
            where T : ISafelyComparable<T>
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// GreaterOrEquals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static bool Greater<T>(T? left, T? right)
            where T : ISafelyComparable<T>
        {
            return Compare(left, right) > 0;
        }

        /// <summary>
        /// GreaterOrEquals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static bool LessOrEquals<T>(T? left, T? right)
            where T : ISafelyComparable<T>
        {
            return Compare(left, right) <= 0;
        }

        /// <summary>
        /// GreaterOrEquals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static bool GreaterOrEquals<T>(T? left, T? right)
            where T : ISafelyComparable<T>
        {
            return Compare(left, right) >= 0;
        }

        /// <summary>
        /// CompareTo
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="obj">Object</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static int CompareTo<T>(T source, object? obj)
            where T : ISafelyComparable<T>
        {
            return obj is T other
                ? Compare(source, other)
                : throw new ArgumentException($"Object should be of type {typeof(T).FullName}");
        }

        /// <summary>
        /// CompareTo
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="other">Other</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static int CompareTo<T>(T source, T? other)
            where T : ISafelyComparable<T>
        {
            return Compare(source, other);
        }

        private static int Compare<T>(T? left, T? right)
            where T : ISafelyComparable<T>
        {
            if (ReferenceEquals(null, left))
            {
                return 1;
            }

            if (ReferenceEquals(null, right))
            {
                return 1;
            }

            return ReferenceEquals(left, right)
                ? 0
                : left.SafeCompareTo(right);
        }
    }
}