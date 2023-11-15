namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;

    /// <summary>
    /// Heap entry
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public class HeapEntry<TElement, TKey> : IEquatable<HeapEntry<TElement, TKey>>,
                                             ISafelyEquatable<HeapEntry<TElement, TKey>>,
                                             ISafelyComparable<HeapEntry<TElement, TKey>>,
                                             IComparable<HeapEntry<TElement, TKey>>,
                                             IComparable
        where TKey : IEquatable<TKey>, IComparable<TKey>, IComparable
    {
        private readonly TKey _key;

        /// <summary>
        /// .cctor
        /// </summary>
        /// <param name="key">Element key</param>
        /// <param name="element">Element</param>
        public HeapEntry(TKey key, TElement element)
        {
            _key = key;
            Element = element;
        }

        /// <summary>
        /// Element
        /// </summary>
        public TElement Element { get; }

        /// <summary>
        /// Equality ==
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator ==(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// Equality !=
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator !=(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <summary>
        /// Less operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator <(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
        {
            return Comparable.Less(left, right);
        }

        /// <summary>
        /// Greater operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator >(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
        {
            return Comparable.Greater(left, right);
        }

        /// <summary>
        /// Less or equals operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator <=(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
        {
            return Comparable.LessOrEquals(left, right);
        }

        /// <summary>
        /// Greater or equals operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator >=(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
        {
            return Comparable.GreaterOrEquals(left, right);
        }

        /// <inheritdoc />
        public int SafeCompareTo(HeapEntry<TElement, TKey> other)
        {
            return _key.CompareTo(other._key);
        }

        /// <inheritdoc />
        public int CompareTo(HeapEntry<TElement, TKey>? other)
        {
            return Comparable.CompareTo(this, other);
        }

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            return Comparable.CompareTo(this, obj);
        }

        /// <inheritdoc />
        public bool SafeEquals(HeapEntry<TElement, TKey> other)
        {
            return _key.Equals(other._key);
        }

        /// <inheritdoc />
        public bool Equals(HeapEntry<TElement, TKey>? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }
    }
}