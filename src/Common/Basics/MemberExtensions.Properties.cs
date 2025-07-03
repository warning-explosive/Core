namespace SpaceEngineers.Core.Basics;

using System.Reflection;
using System.Runtime.CompilerServices;

using Exceptions;

public static partial class MemberExtensions
{
    private const BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;

    private static readonly Type[] IsExternalInitTypes =
    [
        TypeExtensions.FindType("System.Private.CoreLib System.Runtime.CompilerServices.IsExternalInit")
    ];

    public static bool HasProperty(this object target, string propertyName)
    {
        return target
            .GetType()
            .GetProperty(propertyName, PropertyFlags) != null;
    }

    public static object? GetPropertyValue(this object target, string propertyName)
    {
        var type = target.GetType();

        var property = type.GetProperty(propertyName, PropertyFlags | BindingFlags.GetProperty);

        if (property == null)
        {
            throw new NotFoundException($"Property not found {type.Name}.{propertyName}");
        }

        return property.GetValue(target);
    }

    public static TProperty GetPropertyValue<TProperty>(this object target, string propertyName)
    {
        return target.GetPropertyValue(propertyName).EnsureType<TProperty>();
    }

    public static bool IsNullable(this PropertyInfo propertyInfo)
    {
        return IsNullable(propertyInfo, info => info.PropertyType, info => info.GetCustomAttributes());
    }

    public static bool IsEqualityContract(this PropertyInfo property)
    {
        return property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase)
               && property.GetMethod?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
    }

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
        return propertyInfo.SetMethod != null
               && propertyInfo.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any(IsExternalInitTypes.Contains);
    }
}