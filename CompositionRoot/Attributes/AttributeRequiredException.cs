namespace SpaceEngineers.Core.CompositionRoot.Attributes
{
    using System;

    /// <inheritdoc />
    public class AttributeRequiredException : ApplicationException
    {
        /// <summary> .ctor </summary>
        /// <param name="attributeType">Type of required attribute</param>
        public AttributeRequiredException(Type attributeType) : base(attributeType.FullName)
        {
        }
    }
}