namespace SpaceEngineers.Core.Basics;

using System;
using System.Linq;
using System.Reflection;

public static class AccessorExtensions
{
    private static readonly Type[] IsExternalInitTypes =
        new[]
        {
            TypeExtensions.FindType("System.Private.CoreLib System.Runtime.CompilerServices.IsExternalInit"),
        };

    public static bool GetIsAccessible(this PropertyInfo property)
    {
        var getMethod = property.GetGetMethod(true);
        return getMethod != null && getMethod.IsAccessible();
    }

    public static bool SetIsAccessible(this PropertyInfo property)
    {
        var setMethod = property.GetSetMethod(true);
        return setMethod != null && setMethod.IsAccessible();
    }

    public static bool HasInitializer(this PropertyInfo propertyInfo)
    {
        return propertyInfo.SetMethod != default
               && propertyInfo.SetMethod.ReturnParameter != null
               && propertyInfo.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any(IsExternalInitTypes.Contains);
    }
}