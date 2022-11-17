namespace SpaceEngineers.Core.Basics.Exceptions
{
    using System;

    /// <summary>
    /// AttributeRequiredException
    /// Type should be marked by attribute
    /// </summary>
    public sealed class AttributeRequiredException : Exception
    {
        /// <summary> .ctor </summary>
        /// <param name="attributeType">Type of required attribute</param>
        /// <param name="notMarkedType">Type that not marked by attribute</param>
        public AttributeRequiredException(Type attributeType, Type notMarkedType)
            : base($"Type {notMarkedType.FullName} is expected to be marked with {attributeType.FullName} attribute")
        {
        }
    }
}