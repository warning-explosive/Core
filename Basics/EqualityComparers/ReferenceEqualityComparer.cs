namespace SpaceEngineers.Core.Basics.EqualityComparers
{
    using System.Collections.Generic;

    /// <summary>
    /// Object ReferenceEqualityComparer
    /// </summary>
    /// <typeparam name="T">Type-Argument</typeparam>
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