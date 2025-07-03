namespace SpaceEngineers.Core.Basics;

public interface ISafelyComparable<in T> : IComparable<T>, IComparable
{
    int SafeCompareTo(T other);
}

public static class Comparable
{
    public static bool Less<T>(T? left, T? right)
        where T : ISafelyComparable<T>
    {
        return Compare(left, right) < 0;
    }

    public static bool Greater<T>(T? left, T? right)
        where T : ISafelyComparable<T>
    {
        return Compare(left, right) > 0;
    }

    public static bool LessOrEquals<T>(T? left, T? right)
        where T : ISafelyComparable<T>
    {
        return Compare(left, right) <= 0;
    }

    public static bool GreaterOrEquals<T>(T? left, T? right)
        where T : ISafelyComparable<T>
    {
        return Compare(left, right) >= 0;
    }

    public static int CompareTo<T>(T source, object? obj)
        where T : ISafelyComparable<T>
    {
        return obj is T other
            ? Compare(source, other)
            : throw new ArgumentException($"Object should be of type {typeof(T).FullName}");
    }

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