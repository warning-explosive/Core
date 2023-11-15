namespace SpaceEngineers.Core.Basics
{
    /// <summary>
    /// Equatable
    /// </summary>
    public static class Equatable
    {
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="other">Other</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static bool Equals<T>(T? source, T? other)
            where T : ISafelyEquatable<T>
        {
            if (ReferenceEquals(source, other))
            {
                return true;
            }

            if (ReferenceEquals(null, source)
                || ReferenceEquals(null, other))
            {
                return false;
            }

            var typeMatches = source.GetType() == other.GetType();
            var valueMatches = source.SafeEquals(other);

            return typeMatches && valueMatches;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="obj">Object</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Comparison result</returns>
        public static bool Equals<T>(T source, object? obj)
            where T : ISafelyEquatable<T>
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(source, obj))
            {
                return true;
            }

            var typeMatches = source.GetType() == obj.GetType();

            if (!typeMatches)
            {
                return false;
            }

            var valueMatches = source.SafeEquals((T)obj);

            return valueMatches;
        }
    }
}