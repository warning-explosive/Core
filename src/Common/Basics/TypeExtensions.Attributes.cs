namespace SpaceEngineers.Core.Basics;

using Exceptions;

public static partial class TypeExtensions
{
    public static TAttribute GetRequiredAttribute<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return TypeInfoStorage
                   .Get(type)
                   .Attributes
                   .OfType<TAttribute>()
                   .SingleOrDefault(Amb)
               ?? throw new AttributeRequiredException(typeof(TAttribute), type);

        static string Amb(IEnumerable<TAttribute> attributes)
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
            .SingleOrDefault(Amb);

        static string Amb(IEnumerable<TAttribute> attributes)
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
}