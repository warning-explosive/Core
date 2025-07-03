namespace SpaceEngineers.Core.Basics.Heap;

using System;
using Basics;

public partial class HeapEntry<TElement, TKey>(TKey key, TElement element) :
    ISafelyEquatable<HeapEntry<TElement, TKey>>,
    ISafelyComparable<HeapEntry<TElement, TKey>>
    where TKey : IEquatable<TKey>, IComparable<TKey>, IComparable
{
    private readonly TKey _key = key;

    public TElement Element { get; } = element;

    public int SafeCompareTo(HeapEntry<TElement, TKey> other)
    {
        return _key.CompareTo(other._key);
    }

    public bool SafeEquals(HeapEntry<TElement, TKey> other)
    {
        return _key.Equals(other._key);
    }

    public override int GetHashCode()
    {
        return _key.GetHashCode();
    }
}