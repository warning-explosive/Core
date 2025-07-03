namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exceptions;

public static partial class MemberExtensions
{
    public static TAttribute GetRequiredAttribute<TAttribute>(this MemberInfo memberInfo)
        where TAttribute : Attribute
    {
        return memberInfo
                   .GetCustomAttributes<TAttribute>()
                   .SingleOrDefault(Amb)
               ?? throw new AttributeRequiredException(typeof(TAttribute), memberInfo);

        static string Amb(IEnumerable<TAttribute> attributes)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo memberInfo)
        where TAttribute : Attribute
    {
        return memberInfo
            .GetCustomAttributes<TAttribute>()
            .SingleOrDefault(Amb);

        static string Amb(IEnumerable<TAttribute> attributes)
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
                   .SingleOrDefault(Amb)
               ?? throw new AttributeRequiredException(typeof(TAttribute), methodInfo);

        static string Amb(IEnumerable<TAttribute> attributes)
        {
            return $"Type has more than one {typeof(TAttribute)}";
        }
    }

    public static TAttribute? GetAttribute<TAttribute>(this MethodInfo methodInfo)
        where TAttribute : Attribute
    {
        return methodInfo
            .GetCustomAttributes<TAttribute>()
            .SingleOrDefault(Amb);

        static string Amb(IEnumerable<TAttribute> attributes)
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