namespace SpaceEngineers.Core.Basics.Exceptions
{
    using System;

    /// <summary>
    /// TypeMismatchException
    /// Type is not accessable from expected type
    /// </summary>
    public class TypeMismatchException : Exception
    {
        /// <summary> .ctor </summary>
        /// <param name="originalType">Type of required attribute</param>
        /// <param name="targetType">Type that not marked by attribute</param>
        public TypeMismatchException(Type originalType, Type targetType)
            : base($"{originalType.FullName} is not accessable from {targetType.FullName}")
        {
        }
    }
}