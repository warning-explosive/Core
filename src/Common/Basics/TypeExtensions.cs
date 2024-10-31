namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class TypeExtensions
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
                .Invoke(_ => Enumerable.Empty<Type>());
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

    public static bool IsInstanceOfType(this object? obj, Type type)
    {
        return type.IsInstanceOfType(obj);
    }

    public static bool IsCompilerGenerated(this Type type)
    {
        return type.HasAttribute<CompilerGeneratedAttribute>();
    }

    public static object? DefaultValue(this Type type)
    {
        return type.IsValueType
            ? Activator.CreateInstance(type)
            : null;
    }

    public static bool IsCollection(this Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type)
               && !typeof(IQueryable).IsAssignableFrom(type)
               && type != typeof(string);
    }

    public static bool IsArray(this Type type)
    {
        return type.IsArray || type == typeof(Array);
    }

    public static bool IsPrimitive(this Type type)
    {
        /*
         * Is type primitive or not
         * - Boolean
         * - Byte
         * - SByte
         * - Int16
         * - UInt16
         * - Int32
         * - UInt32
         * - Int64
         * - UInt64
         * - IntPtr
         * - UIntPtr
         * - Char
         * - Double
         * - Single
         * - Decimal
         * - Enum
         * - Guid
         * - DateTime
         * - TimeSpan
         * - string
         * - System.Type
         */

        return IsPrimitiveType(type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)));

        static bool IsPrimitiveType(Type t)
        {
            return t.IsPrimitive
                   || t.IsEnum
                   || t == typeof(Guid)
                   || t == typeof(DateTime)
                   || t == typeof(TimeSpan)
                   || t == typeof(decimal)
                   || t == typeof(string)
                   || t == typeof(Type);
        }
    }

    public static bool IsNumeric(this Type type)
    {
        /*
         * Is type numeric or not
         * - Int16
         * - UInt16
         * - Int32
         * - UInt32
         * - Int64
         * - UInt64
         * - Double
         * - Single
         * - Decimal
         */

        return IsNumericType(type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)));

        static bool IsNumericType(Type t)
        {
            return t == typeof(short)
                   || t == typeof(ushort)
                   || t == typeof(int)
                   || t == typeof(uint)
                   || t == typeof(long)
                   || t == typeof(ulong)
                   || t == typeof(float)
                   || t == typeof(double)
                   || t == typeof(decimal);
        }
    }

    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType
               && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsEnumFlags(this Type type)
    {
        return type.IsEnum
               && type.IsDefined(typeof(FlagsAttribute), false);
    }

    public static bool IsReference(this Type type)
    {
        return !type.IsValueType;
    }

    public static bool IsRecord(this Type type)
    {
        if (type.GetMethod("<Clone>$") == null)
        {
            return false;
        }

        return type
            .GetPropertyValue<PropertyInfo[]>("DeclaredProperties")
            .SingleOrDefault(MemberExtensions.IsEqualityContract) != null;
    }

    public static bool IsConstructedOrNonGenericType(this Type type)
    {
        return !type.IsGenericType || type.IsConstructedGenericType;
    }

    public static bool IsPartiallyClosed(this Type type)
    {
        return type.IsGenericType
               && type.ContainsGenericParameters
               && type.GetGenericTypeDefinition() != type;
    }

    public static bool IsConcreteType(this Type type)
    {
        return !type.IsAbstract
               && !type.IsArray()
               && type != typeof(object)
               && !typeof(Delegate).IsAssignableFrom(type);
    }

    public static bool IsDecorator(this ConstructorInfo cctor, Type dependency)
    {
        return cctor
            .SelfDependencies(dependency)
            .Any(type => ContainsDecorateeParameters(cctor, type));

        static bool ContainsDecorateeParameters(ConstructorInfo cctor, Type dependency)
        {
            return cctor
                .GetParameters()
                .Any(parameter => IsDecorateeParameter(parameter, dependency));
        }

        static bool IsDecorateeParameter(ParameterInfo parameter, Type dependency)
        {
            return parameter.ParameterType == dependency;
        }
    }

    public static bool IsComposite(this ConstructorInfo cctor, Type dependency)
    {
        return cctor
            .SelfDependencies(dependency)
            .Any(type => ContainsCompositeParameters(cctor, type));

        static bool ContainsCompositeParameters(ConstructorInfo cctor, Type dependency)
        {
            return cctor
                .GetParameters()
                .Any(parameter => IsCompositeParameter(parameter, dependency));
        }

        static bool IsCompositeParameter(ParameterInfo parameter, Type dependency)
        {
            return parameter
                .ParameterType
                .ExtractGenericArgumentAt(typeof(IEnumerable<>)) == dependency;
        }
    }

    public static bool IsSubclassOfOpenGeneric(this Type type, Type openGenericAncestor)
    {
        if (!openGenericAncestor.IsGenericTypeDefinition)
        {
            return false;
        }

        return TypeInfoStorage.Get(type).GenericTypeDefinitions.Contains(openGenericAncestor)
               || TypeInfoStorage.Get(type).GenericInterfaceDefinitions.Contains(openGenericAncestor);
    }

    public static bool IsContainsInterfaceDeclaration(this Type type, Type @interface)
    {
        if (TypeInfoStorage.Get(type).DeclaredInterfaces.Contains(@interface))
        {
            return true;
        }

        // generic
        var genericTypeDefinition = type.GenericTypeDefinitionOrSelf();
        var genericInterfaceDefinition = @interface.GenericTypeDefinitionOrSelf();

        return TypeInfoStorage.Get(genericTypeDefinition)
                   .DeclaredInterfaces
                   .Select(z => z.GUID)
                   .Contains(genericInterfaceDefinition.GUID)
               && type.GetGenericArguments().SequenceEqual(@interface.GetGenericArguments());
    }

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
            ? Enumerable.Empty<Type>()
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
            : source.ExtractGenericArgumentsAt(openGeneric, typeArgumentAt).SingleOrDefault() ?? source;
    }

    public static IEnumerable<Type[]> ExtractAllGenericArguments(this Type source, Type openGeneric)
    {
        if (!openGeneric.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
        }

        return !IsSubclassOfOpenGeneric(source, openGeneric)
            ? Enumerable.Empty<Type[]>()
            : source
                .IncludedTypes()
                .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                .Select(type => type.GetGenericArguments().ToArray());
    }

    public static Type[] ExtractGenericArguments(this Type source, Type openGeneric)
    {
        return source.ExtractAllGenericArguments(openGeneric).Single();
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
                .InformativeSingle(Amb(openGeneric, source));

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

    public static Type GenericTypeDefinitionOrSelf(this Type type)
    {
        return type.IsGenericType
            ? type.GetGenericTypeDefinition()
            : type;
    }

    public static bool FitsForTypeArgument(this Type typeForCheck, Type typeArgument)
    {
        if (!typeArgument.IsGenericParameter)
        {
            if (!typeArgument.IsGenericType)
            {
                return typeArgument.IsAssignableFrom(typeForCheck);
            }

            var genericParameters = typeArgument.GetGenericArguments();

            return typeForCheck
                .ExtractAllGenericArguments(typeArgument.GetGenericTypeDefinition())
                .Any(set => set
                    .Zip(genericParameters, (arg, param) => (arg, param))
                    .All(pair => pair.arg.FitsForTypeArgument(pair.param)));
        }

        var byConstraints = typeArgument
            .GetGenericParameterConstraints()
            .All(CheckConstraint);

        var filters = GetFiltersByTypeParameterAttributes(typeArgument.GenericParameterAttributes);
        var byGenericParameterAttributes = filters.All(filter => filter(typeForCheck));

        return byConstraints && byGenericParameterAttributes;

        bool CheckConstraint(Type constraint)
        {
            if (!constraint.IsGenericType)
            {
                return constraint.IsAssignableFrom(typeForCheck);
            }

            var constraintGenericTypeDefinition = constraint.GetGenericTypeDefinition();

            return IsSubclassOfOpenGeneric(typeForCheck, constraintGenericTypeDefinition)
                   && HasSuitableTypeArguments();

            bool HasSuitableTypeArguments()
            {
                var typeArgumentsForCheck = ExtractAllGenericArguments(typeForCheck, constraintGenericTypeDefinition).ToArray();

                return constraint
                    .GetGenericArguments()
                    .Select((constraintGenericArgument, position) => (constraintGenericArgument, position))
                    .All(info => typeArgumentsForCheck
                        .Any(typeArgumentForCheck => info.constraintGenericArgument == typeArgument
                            ? typeForCheck == typeArgumentForCheck[info.position] // T : IService<T>
                            : FitsForTypeArgument(typeArgumentForCheck[info.position], info.constraintGenericArgument)));
            }
        }

        static ICollection<Func<Type, bool>> GetFiltersByTypeParameterAttributes(GenericParameterAttributes genericParameterAttributes)
        {
            var filters = new List<Func<Type, bool>>();

            if (genericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                filters.Add(type => type.IsClass || type.IsInterface);
            }

            if (genericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                filters.Add(type => type.IsValueType && !type.IsNullable());
            }

            if (genericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                filters.Add(type =>
                {
                    if (type.IsValueType)
                    {
                        return true;
                    }

                    var ctor = type.GetConstructor(Array.Empty<Type>());

                    return ctor != null;
                });
            }

            if (!filters.Any())
            {
                filters.Add(type => true);
            }

            return filters;
        }
    }

    private static IEnumerable<Type> SelfDependencies(this ConstructorInfo cctor, Type dependency)
    {
        return cctor
            .DeclaringType
            .BaseTypes()
            .Where(type => type.GenericTypeDefinitionOrSelf() == dependency.GenericTypeDefinitionOrSelf());
    }
}