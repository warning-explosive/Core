namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static partial class MemberExtensions
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;

    private static readonly Type NullableAttributeType = TypeExtensions.FindType("System.Private.CoreLib System.Runtime.CompilerServices.NullableAttribute");

    public static bool IsAccessible(this MethodBase method)
    {
        return (method.IsPublic || method.IsAssembly)
               && !(method.IsPrivate || method.IsFamilyOrAssembly);
    }

    public static bool IsEqualityContract(this PropertyInfo property)
    {
        return property.Name.Equals("EqualityContract", StringComparison.OrdinalIgnoreCase)
               && property.GetMethod?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
    }

    public static bool HasProperty(this object target, string propertyName)
    {
        return target
            .GetType()
            .GetProperty(propertyName, Flags) != null;
    }

    public static object GetPropertyValue(this object target, string propertyName)
    {
        return target
            .GetType()
            .GetProperty(propertyName, Flags | BindingFlags.GetProperty)
            .GetValue(target);
    }

    public static TProperty GetPropertyValue<TProperty>(this object target, string propertyName)
    {
        return (TProperty)target.GetPropertyValue(propertyName);
    }

    public static bool HasField(this object target, string fieldName)
    {
        return HasField(target.GetType(), fieldName);
    }

    public static bool HasField(this Type type, string fieldName)
    {
        return type.GetField(fieldName, Flags) != null;
    }

    public static object GetFieldValue(this object target, string fieldName)
    {
        return GetFieldValue(target.GetType(), target, fieldName);
    }

    public static TField GetFieldValue<TField>(this object target, string fieldName)
    {
        return (TField)target.GetFieldValue(fieldName);
    }

    public static object GetFieldValue(this Type type, object target, string fieldName)
    {
        return type.HasField(fieldName)
            ? type.GetField(fieldName, Flags).GetValue(target)
            : throw new InvalidOperationException($"Type '{type}' doesn't contain field '{fieldName}'");
    }

    public static TField GetFieldValue<TField>(this Type type, object target, string fieldName)
    {
        return (TField)type.GetFieldValue(target, fieldName);
    }

    public static bool IsNullable(this PropertyInfo propertyInfo)
    {
        return IsNullable(propertyInfo, info => info.PropertyType, info => info.GetCustomAttributes());
    }

    public static bool IsNullable(this FieldInfo fieldInfo)
    {
        return IsNullable(fieldInfo, info => info.FieldType, info => info.GetCustomAttributes());
    }

    private static bool IsNullable<T>(
        this T memberInfo,
        Func<T, Type> typeAccessor,
        Func<T, IEnumerable<Attribute>> attributesAccessor)
        where T : MemberInfo
    {
        var isNullableValueType = typeAccessor(memberInfo).IsNullable();

        if (isNullableValueType)
        {
            return true;
        }

        return attributesAccessor(memberInfo).Any(NullableAttributeType.IsInstanceOfType);
    }
}