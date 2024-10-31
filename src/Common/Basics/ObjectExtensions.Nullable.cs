namespace SpaceEngineers.Core.Basics;

using Exceptions;

public static partial class ObjectExtensions
{
    public static TExpected EnsureType<TExpected>(this object? input)
    {
        if (input is TExpected expected)
        {
            return expected;
        }

        throw new TypeMismatchException(typeof(TExpected), input.GetType());
    }
}