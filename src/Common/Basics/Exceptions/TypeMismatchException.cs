namespace SpaceEngineers.Core.Basics.Exceptions;

using System;

public sealed class TypeMismatchException : Exception
{
    public TypeMismatchException(Type expectedType, Type actualType)
        : base($"{expectedType.FullName} is not accessible from {actualType.FullName}")
    {
    }
}