namespace SpaceEngineers.Core.Basics;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public static partial class TypeExtensions
{
    public static IEnumerable<Type> AllTypes()
    {
        return AssemblyExtensions
            .AllAssembliesFromCurrentDomain()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(GetTypes);

        static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            return ExecutionExtensions
                .Try<IEnumerable<Type>>(assembly.GetTypes)
                .Catch<ReflectionTypeLoadException>()
                .Catch<FileNotFoundException>()
                .Invoke(_ => []);
        }
    }

    public static Type FindType(this TypeNode typeNode)
    {
        return typeNode;
    }

    public static bool TryFindType(TypeNode typeNode, [NotNullWhen(true)] out Type? type)
    {
        try
        {
            type = FindType(typeNode);
            return true;
        }
        catch (Exception)
        {
            type = null;
            return false;
        }
    }

    public static IEnumerable<Type> BaseTypes(this Type source)
    {
        /*
         * Base types from source type
         * - base types
         * - interfaces
         */

        return TypeInfoStorage.Get(source).BaseTypes
            .Concat(source.GetInterfaces());
    }

    public static IEnumerable<Type> IncludedTypes(this Type source)
    {
        /*
         * Types included in source type
         * - source
         * - base types
         * - interfaces
         */

        return new[] { source }.Concat(source.BaseTypes());
    }

    public static IReadOnlyCollection<Type> DerivedTypes(this Type source)
    {
        return TypeInfoStorage.Get(source).DerivedTypes;
    }

    public static Type ApplyGenericArguments(this Type openGeneric, Type source)
    {
        if (openGeneric.IsConstructedOrNonGenericType())
        {
            return openGeneric;
        }

        if (openGeneric.IsGenericType
            && openGeneric.IsGenericTypeDefinition
            && source.IsConstructedOrNonGenericType())
        {
            var genericArguments = source
                .ExtractAllGenericArguments(openGeneric)
                .Single(Amb(openGeneric, source));

            return openGeneric.MakeGenericType(genericArguments);
        }

        throw new InvalidOperationException($"Type {openGeneric.FullName} can't be closed from {source.FullName}");

        static Func<IEnumerable<Type[]>, string> Amb(Type openGeneric, Type source)
        {
            return args =>
            {
                var details = args
                    .Select(genericArguments => genericArguments
                        .Select(arg => arg.Name)
                        .ToString(", "))
                    .ToString("; ");

                return $"Type {source.FullName} has different implementations of {openGeneric.FullName}: {details}";
            };
        }
    }
}