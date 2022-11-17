namespace SpaceEngineers.Core.Basics.Exceptions
{
    using System;

    /// <summary>
    /// TypeMismatchException
    /// Type is not accessible from expected type
    /// </summary>
    public sealed class TypeMismatchException : Exception
    {
        /// <summary> .ctor </summary>
        /// <param name="expectedType">Expected type</param>
        /// <param name="actualType">Actual type</param>
        public TypeMismatchException(Type expectedType, Type actualType)
            : base($"{expectedType.FullName} is not accessible from {actualType.FullName}")
        {
        }
    }
}