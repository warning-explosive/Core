namespace SpaceEngineers.Core.Basics;

using System.Collections.Generic;
using System.Linq;

public static class DeconstructExtensions
{
    public static void Deconstruct<TKey, TValue>(
        this IGrouping<TKey, TValue> grouping,
        out TKey key,
        out IEnumerable<TValue> values)
    {
        key = grouping.Key;
        values = grouping;
    }

    public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out IEnumerable<T> rest)
    {
        first = source.FirstOrDefault();
        rest = source.Skip(1);
    }

    public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out T second, out IEnumerable<T> rest)
        => (first, (second, rest)) = source;

    public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out T second, out T third, out IEnumerable<T> rest)
        => (first, second, (third, rest)) = source;

    public static IEnumerable<T> ConstructEnumerable<T>(this (T first, T second) source)
    {
        var (first, second) = source;

        yield return first;
        yield return second;
    }

    public static IEnumerable<T> ConstructEnumerable<T>(this (T first, T second, T third) source)
    {
        var (first, second, third) = source;

        yield return first;
        yield return second;
        yield return third;
    }
}