namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exceptions;

public static class EnumerableExtensions
{
    public static IEnumerable<TValue> DistinctBy<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        return source
            .GroupBy(keySelector, comparer)
            .Select(it => it.First());
    }

    public static IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> Stack<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector)
    {
        return source
            .Aggregate(new Stack<KeyValuePair<TKey, List<TValue>>>(), Aggregate)
            .Reverse()
            .Select(pair => new KeyValuePair<TKey, IEnumerable<TValue>>(pair.Key, pair.Value));

        Stack<KeyValuePair<TKey, List<TValue>>> Aggregate(Stack<KeyValuePair<TKey, List<TValue>>> acc, TValue next)
        {
            var key = keySelector(next);

            if (!acc.TryPeek(out var peek)
                || !EqualityComparer<TKey>.Default.Equals(key, peek.Key))
            {
                acc.Push(new KeyValuePair<TKey, List<TValue>>(key, new List<TValue> { next }));
            }
            else
            {
                peek.Value.Add(next);
            }

            return acc;
        }
    }

    public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(
        this IEnumerable<TLeft> leftSource,
        IEnumerable<TRight> rightSource,
        Func<TLeft, TKey> leftKeySelector,
        Func<TRight, TKey> rightKeySelector,
        Func<TLeft?, TRight?, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        var leftLookup = leftSource.ToLookup(leftKeySelector);
        var rightLookup = rightSource.ToLookup(rightKeySelector);

        var keys = leftLookup
            .Select(p => p.Key)
            .Concat(rightLookup.Select(p => p.Key))
            .ToHashSet(comparer ?? EqualityComparer<TKey>.Default);

        return from key in keys
            from left in leftLookup[key].DefaultIfEmpty()
            from right in rightLookup[key].DefaultIfEmpty()
            select resultSelector(left, right);
    }

    public static IEnumerable<TResult> LeftJoin<TLeft, TRight, TKey, TResult>(
        this IEnumerable<TLeft> leftSource,
        IEnumerable<TRight> rightSource,
        Func<TLeft, TKey> leftKeySelector,
        Func<TRight, TKey> rightKeySelector,
        Func<TLeft, TRight?, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        var leftLookup = leftSource.ToLookup(leftKeySelector);
        var rightLookup = rightSource.ToLookup(rightKeySelector);

        var keys = leftLookup
            .Select(p => p.Key)
            .ToHashSet(comparer ?? EqualityComparer<TKey>.Default);

        return from key in keys
            from left in leftLookup[key]
            from right in rightLookup[key].DefaultIfEmpty()
            select resultSelector(left, right);
    }

    public static IEnumerable<T> Flatten<T>(
        this T source,
        Func<T, IEnumerable<T>> unfold)
    {
        return new[] { source }.Concat(unfold(source).SelectMany(z => Flatten(z, unfold)));
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> unfold)
    {
        return source.SelectMany(item => item.Flatten(unfold));
    }

    // TODO: what is this
    public static IEnumerable<ICollection<T>> ColumnsCartesianProduct<T>(this IEnumerable<IEnumerable<T>> sourceColumns)
    {
        if (!sourceColumns.Any())
        {
            return Enumerable.Empty<ICollection<T>>();
        }

        IEnumerable<ICollection<T>> seed = sourceColumns
            .Take(1)
            .Single()
            .Select(item => new List<T> { item });

        return sourceColumns
            .Skip(1)
            .Aggregate(seed, Aggregate);

        static IEnumerable<ICollection<T>> Aggregate(IEnumerable<ICollection<T>> acc, IEnumerable<T> next)
        {
            return acc.Join(next,
                _ => true,
                _ => true,
                (left, right) => new List<T>(left) { right });
        }
    }

    public static void Each<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static void Each<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
    {
        using (var enumerator = source.GetEnumerator())
        {
            var i = 0;

            while (enumerator.MoveNext())
            {
                action(enumerator.Current, i++);
            }
        }
    }

    public static IEnumerable<T> AsEnumerable<T>(this IEnumerable enumerable)
    {
        return enumerable.GetEnumerator().AsEnumerable<T>();
    }

    public static IEnumerable<T> AsEnumerable<T>(this IEnumerator numerator)
    {
        while (numerator.MoveNext())
        {
            if (numerator.Current is T typed)
            {
                yield return typed;
            }
        }
    }

    public static IOrderedEnumerable<T> AsOrderedEnumerable<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => 1);
    }

    public static IOrderedEnumerable<TSource> OrderByDependencies<TSource, TDependency>(
        this IEnumerable<TSource> source,
        Func<TSource, TDependency> getKey,
        Func<TSource, IEnumerable<TSource>> getDependencies)
    {
        return source.OrderBy(SortFunc);

        int SortFunc(TSource item)
        {
            var key = getKey(item);
            var dependencies = getDependencies(item).ToList();
            var dependenciesKeys = dependencies.Select(getKey).ToList();

            var depth = 0;

            while (dependenciesKeys.Any())
            {
                if (dependenciesKeys.Contains(key))
                {
                    throw new InvalidOperationException($"{key} has cycle dependency");
                }

                ++depth;

                dependencies = dependencies.SelectMany(getDependencies).ToList();
                dependenciesKeys = dependencies.Select(getKey).ToList();
            }

            return depth;
        }
    }

    public static T InformativeSingle<T>(this IEnumerable<T> source, Func<IEnumerable<T>, string> amb)
    {
        var items = source.Take(2).ToList();

        if (!items.Any())
        {
            throw new NotFoundException("Source collection is empty");
        }

        if (items.Count != 1)
        {
            throw new AmbiguousMatchException(amb(items));
        }

        return items.Single();
    }

    public static T InformativeSingle<T, TState>(this IEnumerable<T> source, Func<TState, IEnumerable<T>, string> amb, TState state)
    {
        var items = source.Take(2).ToList();

        if (!items.Any())
        {
            throw new NotFoundException("Source collection is empty");
        }

        if (items.Count != 1)
        {
            throw new AmbiguousMatchException(amb(state, items));
        }

        return items.Single();
    }

    public static T InformativeSingleOrDefault<T>(this IEnumerable<T> source, Func<IEnumerable<T>, string> amb)
    {
        var items = source.Take(2).ToList();

        if (items.Count >= 2)
        {
            throw new AmbiguousMatchException(amb(items));
        }

        return items.SingleOrDefault();
    }

    public static T InformativeSingleOrDefault<T, TState>(this IEnumerable<T> source, Func<TState, IEnumerable<T>, string> amb, TState state)
    {
        var items = source.Take(2).ToList();

        if (items.Count >= 2)
        {
            throw new AmbiguousMatchException(amb(state, items));
        }

        return items.SingleOrDefault();
    }
}