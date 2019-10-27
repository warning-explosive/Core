namespace SpaceEngineers.Core.CompositionRoot.Exceptions
{
    using System;

    /// <inheritdoc />
    public class AttributeRequiredException : ApplicationException
    {
        /// <summary> .ctor </summary>
        /// <param name="attributeType">Type of required attribute</param>
        /// <param name="notMarkedType">Type that not marked by attribute</param>
        public AttributeRequiredException(Type attributeType, Type notMarkedType) : base($"{attributeType.FullName}|{notMarkedType.FullName}")
        {
        }
    }
}