namespace SpaceEngineers.Core.Basics.Heap;

using System;
using Basics;

public class HeapEntry<TElement, TKey> : IEquatable<HeapEntry<TElement, TKey>>,
                                         ISafelyEquatable<HeapEntry<TElement, TKey>>,
                                         ISafelyComparable<HeapEntry<TElement, TKey>>,
                                         IComparable<HeapEntry<TElement, TKey>>,
                                         IComparable
    where TKey : IEquatable<TKey>, IComparable<TKey>, IComparable
{
    private readonly TKey _key;

    public HeapEntry(TKey key, TElement element)
    {
        _key = key;
        Element = element;
    }

    public TElement Element { get; }

    public static bool operator ==(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
    {
        return Equatable.Equals(left, right);
    }

    public static bool operator !=(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
    {
        return !Equatable.Equals(left, right);
    }

    public static bool operator <(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
    {
        return Comparable.Less(left, right);
    }

    public static bool operator >(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
    {
        return Comparable.Greater(left, right);
    }

    public static bool operator <=(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
    {
        return Comparable.LessOrEquals(left, right);
    }

    public static bool operator >=(HeapEntry<TElement, TKey>? left, HeapEntry<TElement, TKey>? right)
    {
        return Comparable.GreaterOrEquals(left, right);
    }

    public int SafeCompareTo(HeapEntry<TElement, TKey> other)
    {
        return _key.CompareTo(other._key);
    }

    public int CompareTo(HeapEntry<TElement, TKey>? other)
    {
        return Comparable.CompareTo(this, other);
    }

    public int CompareTo(object? obj)
    {
        return Comparable.CompareTo(this, obj);
    }

    public bool SafeEquals(HeapEntry<TElement, TKey> other)
    {
        return _key.Equals(other._key);
    }

    public bool Equals(HeapEntry<TElement, TKey>? other)
    {
        return Equatable.Equals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return Equatable.Equals(this, obj);
    }

    public override int GetHashCode()
    {
        return _key.GetHashCode();
    }
}