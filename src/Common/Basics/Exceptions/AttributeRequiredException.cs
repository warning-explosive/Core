namespace SpaceEngineers.Core.Basics.Exceptions;

using System;
using System.Reflection;

public sealed class AttributeRequiredException : Exception
{
    public AttributeRequiredException(Type attributeType, Type notMarkedType)
        : base($"Type {notMarkedType.FullName} is expected to be marked with {attributeType.FullName} attribute")
    {
    }

    public AttributeRequiredException(Type attributeType, MemberInfo notMarkedMember)
        : base($"Type {notMarkedMember.DeclaringType?.FullName}.{notMarkedMember.Name} is expected to be marked with {attributeType.FullName} attribute")
    {
    }

    public AttributeRequiredException(Type attributeType, MethodInfo notMarkedMethod)
        : base($"Type {notMarkedMethod.DeclaringType?.FullName}.{notMarkedMethod.Name} is expected to be marked with {attributeType.FullName} attribute")
    {
    }
}