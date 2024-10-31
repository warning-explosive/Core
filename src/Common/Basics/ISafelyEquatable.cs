namespace SpaceEngineers.Core.Basics;

public interface ISafelyEquatable<T>
{
    bool SafeEquals(T other);
}