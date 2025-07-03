namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static partial class ObjectExtensions
{
    public static string ToString<TSource>(
        this IEnumerable<TSource> source,
        string separator,
        Func<TSource, string>? projection = null)
        where TSource : notnull
    {
        projection ??= static item => item as string ?? item.ToString() ?? item.GetType().Name;

        return string.Join(separator, source.Select(projection));
    }

    public static string ToString(
        this (object first, object second) source,
        string separator,
        Func<object, string>? projection = null)
    {
        return source
            .ConstructEnumerable()
            .ToString(separator, projection);
    }

    public static string ToString<TSource>(
        this (TSource first, TSource second) source,
        string separator,
        Func<TSource, string>? projection = null)
        where TSource : notnull
    {
        return source
            .ConstructEnumerable()
            .ToString(separator, projection);
    }

    public static string ToString<TSource>(
        this (TSource first, TSource second, TSource third) source,
        string separator,
        Func<TSource, string>? projection = null)
        where TSource : notnull
    {
        return source
            .ConstructEnumerable()
            .ToString(separator, projection);
    }

    public static string ToString(
        this (object first, object second, object third) source,
        string separator,
        Func<object, string>? projection = null)
    {
        return source
            .ConstructEnumerable()
            .ToString(separator, projection);
    }

    // TODO: remove or move to test API
    public static string Dump(this object instance, BindingFlags flags)
    {
        return DumpValue(instance, flags, 0, []).ToString(Environment.NewLine);

        static IEnumerable<string> DumpValue(
            object? value,
            BindingFlags flags,
            int depth,
            HashSet<object> visited)
        {
            if (value != null && value.GetType().IsCollection())
            {
                var enumerator = ((IEnumerable)value).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    foreach (var str in DumpValue(enumerator.Current, flags, depth, visited))
                    {
                        yield return str;
                    }
                }
            }
            else if (value != null && !value.GetType().IsPrimitive() && visited.Add(value))
            {
                var properties = value.GetType().GetProperties(flags);

                foreach (var property in properties)
                {
                    yield return $"{new string('\t', depth)}{property.Name}";

                    foreach (var str in DumpValue(property.GetValue(value), flags, depth + 1, visited))
                    {
                        yield return str;
                    }
                }

                visited.Remove(value);
            }
            else
            {
                yield return $"{new string('\t', depth)}{value?.ToString() ?? "null"}";
            }
        }
    }
}