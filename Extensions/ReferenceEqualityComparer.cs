namespace SpaceEngineers.Core.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Object ReferenceEqualityComparer
    /// </summary>
    public class ReferenceEqualityComparer<T> : EqualityComparer<T>
        where T : class
    {
        /// <inheritdoc />
        public override bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        /// <inheritdoc />
        public override int GetHashCode(T obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}