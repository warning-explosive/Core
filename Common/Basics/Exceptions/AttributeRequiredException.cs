namespace SpaceEngineers.Core.Basics.Exceptions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// AttributeRequiredException
    /// Type should be marked by attribute
    /// </summary>
    public sealed class AttributeRequiredException : Exception
    {
        /// <summary> .ctor </summary>
        /// <param name="attributeType">Type of required attribute</param>
        /// <param name="notMarkedType">Type that isn't marked by attribute</param>
        public AttributeRequiredException(Type attributeType, Type notMarkedType)
            : base($"Type {notMarkedType.FullName} is expected to be marked with {attributeType.FullName} attribute")
        {
        }

        /// <summary> .ctor </summary>
        /// <param name="attributeType">Type of required attribute</param>
        /// <param name="notMarkedMember">Member that isn't marked by attribute</param>
        public AttributeRequiredException(Type attributeType, MemberInfo notMarkedMember)
            : base($"Type {notMarkedMember.DeclaringType?.FullName}.{notMarkedMember.Name} is expected to be marked with {attributeType.FullName} attribute")
        {
        }

        /// <summary> .ctor </summary>
        /// <param name="attributeType">Type of required attribute</param>
        /// <param name="notMarkedMethod">method that isn't marked by attribute</param>
        public AttributeRequiredException(Type attributeType, MethodInfo notMarkedMethod)
            : base($"Type {notMarkedMethod.DeclaringType?.FullName}.{notMarkedMethod.Name} is expected to be marked with {attributeType.FullName} attribute")
        {
        }
    }
}