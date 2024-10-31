namespace SpaceEngineers.Core.Basics;

public interface ISafelyComparable<in T>
{
    int SafeCompareTo(T other);
}