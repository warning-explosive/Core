namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exceptions;

public static class AttributesExtensions
{
    public static TAttribute GetRequiredAttribute<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return TypeInfoStorage
                   .Get(type)
                   .Attributes
                   .OfType<TAttribute>()
                   .InformativeSingleOrDefault(Amb)
               ?? throw new AttributeRequiredException(typeof(TAttribute), type);

        static string Amb(IEnumerable<TAttribute> arg)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return TypeInfoStorage
            .Get(type)
            .Attributes
            .OfType<TAttribute>();
    }

    public static TAttribute? GetAttribute<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return TypeInfoStorage
            .Get(type)
            .Attributes
            .OfType<TAttribute>()
            .InformativeSingleOrDefault(Amb);

        static string Amb(IEnumerable<TAttribute> arg)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static bool HasAttribute<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return TypeInfoStorage
            .Get(type)
            .Attributes
            .OfType<TAttribute>()
            .Any();
    }

    public static bool HasAttribute(this Type type, Type attributeType)
    {
        return TypeInfoStorage
            .Get(type)
            .Attributes
            .Any(attributeType.IsInstanceOfType);
    }

    public static TAttribute GetRequiredAttribute<TAttribute>(this MemberInfo memberInfo)
        where TAttribute : Attribute
    {
        return memberInfo
                   .GetCustomAttributes<TAttribute>()
                   .InformativeSingleOrDefault(Amb)
               ?? throw new AttributeRequiredException(typeof(TAttribute), memberInfo);

        static string Amb(IEnumerable<TAttribute> arg)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo memberInfo)
        where TAttribute : Attribute
    {
        return memberInfo
            .GetCustomAttributes<TAttribute>()
            .InformativeSingleOrDefault(Amb);

        static string Amb(IEnumerable<TAttribute> arg)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo)
        where TAttribute : Attribute
    {
        return memberInfo.GetCustomAttributes<TAttribute>().Any();
    }

    public static TAttribute GetRequiredAttribute<TAttribute>(this MethodInfo methodInfo)
        where TAttribute : Attribute
    {
        return methodInfo
                   .GetCustomAttributes<TAttribute>()
                   .InformativeSingleOrDefault(Amb)
               ?? throw new AttributeRequiredException(typeof(TAttribute), methodInfo);

        static string Amb(IEnumerable<TAttribute> arg)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static TAttribute? GetAttribute<TAttribute>(this MethodInfo methodInfo)
        where TAttribute : Attribute
    {
        return methodInfo
            .GetCustomAttributes<TAttribute>()
            .InformativeSingleOrDefault(Amb);

        static string Amb(IEnumerable<TAttribute> arg)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static bool HasAttribute<TAttribute>(this MethodInfo methodInfo)
        where TAttribute : Attribute
    {
        return methodInfo.GetCustomAttributes<TAttribute>().Any();
    }
}