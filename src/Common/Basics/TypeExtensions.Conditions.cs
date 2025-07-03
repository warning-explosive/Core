namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static partial class TypeExtensions
{
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
         *
         * - Boolean
         *
         * - Enum
         *
         * - Guid
         *
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
         *
         * - Double
         * - Single
         * - Decimal
         *
         * - DateTimeOffset
         * - DateTime
         * - DateOnly
         * - TimeSpan
         * - TimeOnly
         *
         * - Char
         * - string
         *
         * - System.Type
         */

        return IsPrimitiveType(type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)));

        static bool IsPrimitiveType(Type t)
        {
            return t == typeof(bool)

                   || t.IsEnum

                   || t == typeof(Guid)

                   || t == typeof(byte)
                   || t == typeof(sbyte)
                   || t == typeof(short)
                   || t == typeof(ushort)
                   || t == typeof(int)
                   || t == typeof(uint)
                   || t == typeof(long)
                   || t == typeof(ulong)
                   || t == typeof(nint)
                   || t == typeof(nuint)

                   || t == typeof(double)
                   || t == typeof(float)
                   || t == typeof(decimal)

                   || t == typeof(DateTimeOffset)
                   || t == typeof(DateTime)
                   || t == typeof(DateOnly)
                   || t == typeof(TimeSpan)
                   || t == typeof(TimeOnly)

                   || t == typeof(char)
                   || t == typeof(string)

                   || t == typeof(Type);
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
            .Where(MemberExtensions.IsEqualityContract)
            .SingleOrDefault(Amb, type) != null;

        static string Amb(Type type, IEnumerable<PropertyInfo> properties)
        {
            return $"Type {type} contains more than one EqualityContracts";
        }
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

    public static bool IsDecorator(this Type candidate, Type decoratee)
    {
        return candidate
            .GetConstructors()
            .Any(cctor => IsCctorDecoratesDependency(cctor, decoratee));

        static bool IsCctorDecoratesDependency(ConstructorInfo cctor, Type dependency)
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

                    var ctor = type.GetConstructor([]);

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
            .DeclaringType!
            .BaseTypes()
            .Where(type => type.GenericTypeDefinitionOrSelf() == dependency.GenericTypeDefinitionOrSelf());
    }
}