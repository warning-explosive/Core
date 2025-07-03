namespace SpaceEngineers.Core.Basics.EqualityComparers;

using System.Collections.Generic;

public class ReferenceEqualityComparer<T> : EqualityComparer<T>
    where T : class
{
    public override bool Equals(T? x, T? y)
    {
        return ReferenceEquals(x, y);
    }

    public override int GetHashCode(T? obj)
    {
        return obj == null ? 0 : obj.GetHashCode();
    }
}