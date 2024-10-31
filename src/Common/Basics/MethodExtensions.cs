namespace SpaceEngineers.Core.Basics;

using System;
using System.Reflection;
using Reflection;

public static class MethodExtensions
{
    public static MethodExecutionInfo CallMethod(this Type declaringType, string methodName)
    {
        return new MethodExecutionInfo(declaringType, methodName);
    }

    public static MethodExecutionInfo CallMethod(this object target, string methodName)
    {
        return new MethodExecutionInfo(target.GetType(), methodName).ForInstance(target);
    }

    public static MethodInfo GenericMethodDefinitionOrSelf(this MethodInfo method)
    {
        return method.IsGenericMethod
            ? method.GetGenericMethodDefinition()
            : method;
    }
}