namespace SpaceEngineers.Core.Basics;

using System.Reflection;

using Reflection;

public static partial class MemberExtensions
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

    public static bool IsAccessible(this MethodBase method)
    {
        return (method.IsPublic || method.IsAssembly)
               && !(method.IsPrivate || method.IsFamilyOrAssembly);
    }
}