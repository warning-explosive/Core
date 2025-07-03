namespace SpaceEngineers.Core.Basics;

public static partial class TypeExtensions
{
    public static IEnumerable<Type> ExtractGenericArgumentsAt(this Type source, Type openGeneric, int typeArgumentAt = 0)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
        }

        if (typeArgumentAt < 0 || typeArgumentAt >= openGeneric.GetGenericArguments().Length)
        {
            throw new ArgumentException("Should be in bounds of generic arguments count", nameof(typeArgumentAt));
        }

        return !IsSubclassOfOpenGeneric(source, openGeneric)
            ? []
            : source
                .IncludedTypes()
                .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                .Select(type => type.GetGenericArguments()[typeArgumentAt])
                .Distinct();
    }

    public static Type ExtractGenericArgumentAt(this Type source, Type openGeneric, int typeArgumentAt = 0)
    {
        return source.ExtractGenericArgumentsAt(openGeneric, typeArgumentAt).Single();
    }

    public static Type ExtractGenericArgumentAtOrSelf(this Type source, Type openGeneric, int typeArgumentAt = 0)
    {
        return openGeneric == typeof(Nullable<>)
            ? Nullable.GetUnderlyingType(source) ?? source
            : source.ExtractGenericArgumentsAt(openGeneric, typeArgumentAt).SingleOrDefault(Amb, (source, openGeneric)) ?? source;

        static string Amb((Type, Type) parameters, IEnumerable<Type> types)
        {
            (Type source, Type openGeneric) = parameters;

            return $"Type {source} implements {openGeneric} more than once";
        }
    }

    public static IEnumerable<Type[]> ExtractAllGenericArguments(this Type source, Type openGeneric)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
        }

        return !IsSubclassOfOpenGeneric(source, openGeneric)
            ? []
            : source
                .IncludedTypes()
                .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                .Select(type => type.GetGenericArguments().ToArray());
    }

    public static Type[] ExtractGenericArguments(this Type source, Type openGeneric)
    {
        return source.ExtractAllGenericArguments(openGeneric).Single();
    }

    public static Type GenericTypeDefinitionOrSelf(this Type type)
    {
        return type.IsGenericType
            ? type.GetGenericTypeDefinition()
            : type;
    }
}