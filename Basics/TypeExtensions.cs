namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exceptions;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets default value of the specified system type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Default value</returns>
        public static object? DefaultValue(this Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        /// <exception cref="NotFoundException">Throws if source is empty</exception>
        /// <exception cref="AmbiguousMatchException">Throws if source contains more than one element</exception>
        public static TAttribute GetRequiredAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                       .Get(type)
                       .Attributes
                       .OfType<TAttribute>()
                       .InformativeSingleOrDefault(Amb)
                   ?? throw new AttributeRequiredException(typeof(TAttribute), type);

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Get specified attribute from type
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute</returns>
        /// <exception cref="NotFoundException">Throws if source is empty</exception>
        /// <exception cref="AmbiguousMatchException">Throws if source contains more than one element</exception>
        public static TAttribute? GetAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .OfType<TAttribute>()
                .InformativeSingleOrDefault(Amb);

            string Amb(IEnumerable<TAttribute> arg)
            {
                return $"Type has more than one {typeof(TAttribute)}";
            }
        }

        /// <summary>
        /// Does the specified type has an attribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <typeparam name="TAttribute">TAttribute type-argument</typeparam>
        /// <returns>Attribute existence</returns>
        public static bool HasAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .OfType<TAttribute>()
                .Any();
        }

        /// <summary>
        /// Does the specified type has an attribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="attributeType">Attribute type</param>
        /// <returns>Attribute existence</returns>
        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return TypeInfoStorage
                .Get(type)
                .Attributes
                .Any(attributeType.IsInstanceOfType);
        }

        /// <summary> Order types collection by DependencyAttribute </summary>
        /// <param name="source">Unordered types collection</param>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered by DependencyAttribute collection</returns>
        public static IOrderedEnumerable<Type> OrderByDependencyAttribute(this IEnumerable<Type> source)
        {
            return source.OrderByDependencyAttribute(t => t);
        }

        /// <summary> Order collection by DependencyAttribute </summary>
        /// <param name="source">Unordered source collection</param>
        /// <param name="accessor">Type accessor</param>
        /// <typeparam name="T">Source items type-argument</typeparam>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered by DependencyAttribute collection</returns>
        public static IOrderedEnumerable<T> OrderByDependencyAttribute<T>(this IEnumerable<T> source, Func<T, Type> accessor)
        {
            int SortFunc(T item)
            {
                var type = accessor(item);
                var dependencies = GetDependenciesByAttribute(type);

                var depth = 0;

                while (dependencies.Any())
                {
                    if (dependencies.Contains(type))
                    {
                        throw new InvalidOperationException($"{type} has cycle dependency");
                    }

                    ++depth;

                    dependencies = dependencies.SelectMany(GetDependenciesByAttribute);
                }

                return depth;
            }

            return source.OrderBy(SortFunc);
        }

        /// <summary>
        /// Get type dependencies from DependencyAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type dependencies</returns>
        public static IEnumerable<Type> GetDependenciesByAttribute(this Type type)
        {
            return TypeInfoStorage.Get(type).Dependencies;
        }

        /// <summary>
        ///  Does type implement Nullable
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <returns>Result of check</returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericAncestor">Open generic ancestor</param>
        /// <returns>Result of check</returns>
        public static bool IsSubclassOfOpenGeneric(this Type type, Type openGenericAncestor)
        {
            if (!openGenericAncestor.IsGenericTypeDefinition)
            {
                return false;
            }

            return TypeInfoStorage.Get(type).GenericTypeDefinitions.Contains(openGenericAncestor)
                || TypeInfoStorage.Get(type).GenericInterfaceDefinitions.Contains(openGenericAncestor);
        }

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="interface">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
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

        /// <summary>
        /// Extract type-arguments from derived class of open-generic type at specified index
        /// Distinct, might be several implementations with different type-arguments
        /// </summary>
        /// <param name="source">Source type for extraction</param>
        /// <param name="openGeneric">Open-generic which derived</param>
        /// <param name="typeArgumentAt">Index of type-argument</param>
        /// <returns>Collection of type arguments at specified index.</returns>
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
                       : source.IncludedTypes()
                               .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                               .Select(type => type.GetGenericArguments()[typeArgumentAt])
                               .Distinct();
        }

        /// <summary>
        /// Extract all type-arguments from derived class of open-generic type
        /// </summary>
        /// <param name="source">Source type for extraction</param>
        /// <param name="openGeneric">Open-generic which derived</param>
        /// <returns>Collection of type arguments at specified index.</returns>
        public static IEnumerable<Type[]> ExtractGenericArguments(this Type source, Type openGeneric)
        {
            if (!openGeneric.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Should be GenericTypeDefinition", nameof(openGeneric));
            }

            return !IsSubclassOfOpenGeneric(source, openGeneric)
                       ? Enumerable.Empty<Type[]>()
                       : source.IncludedTypes()
                               .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                               .Select(type => type.GetGenericArguments().ToArray());
        }

        /// <summary>
        /// Unwrap type parameter is possible
        /// </summary>
        /// <param name="typeToUnwrap">Type to unwrap</param>
        /// <param name="openGeneric">Open generic wrapper</param>
        /// <param name="typeArgumentAt">Type argument position</param>
        /// <returns>Unwrapped type</returns>
        public static Type UnwrapTypeParameter(this Type typeToUnwrap, Type openGeneric, int typeArgumentAt = 0)
        {
            return typeToUnwrap.ExtractGenericArgumentsAt(openGeneric, typeArgumentAt).SingleOrDefault()
                   ?? typeToUnwrap;
        }

        /// <summary>
        /// Types included in source type
        /// - source
        /// - base types
        /// - interfaces
        /// </summary>
        /// <param name="source">Source type</param>
        /// <returns>Included types</returns>
        public static IEnumerable<Type> IncludedTypes(this Type source)
        {
            return new[] { source }
                  .Concat(TypeInfoStorage.Get(source).BaseTypes)
                  .Concat(source.GetInterfaces());
        }

        /// <summary>
        /// Extract GenericTypeDefinition or return argument type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>GenericTypeDefinition or argument type</returns>
        public static Type GenericTypeDefinitionOrSelf(this Type type)
        {
            return type.IsGenericType
                       ? type.GetGenericTypeDefinition()
                       : type;
        }

        /// <summary>
        /// True for non-generic or constructed generic types
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is constructed or simple type</returns>
        public static bool IsConstructedOrSimpleType(this Type type)
        {
            return !type.IsGenericType || type.IsConstructedGenericType;
        }

        /// <summary>
        /// Does type fits for type argument
        /// </summary>
        /// <param name="typeForCheck">Actual type for check</param>
        /// <param name="typeArgument">Type argument</param>
        /// <returns>True - fits / False doesn't fits</returns>
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
                    .ExtractGenericArguments(typeArgument.GetGenericTypeDefinition())
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

            bool CheckConstraint(Type constraint) =>
                constraint.IsGenericType
                    ? IsSubclassOfOpenGeneric(typeForCheck, constraint.GetGenericTypeDefinition())
                    : constraint.IsAssignableFrom(typeForCheck);
        }

        private static ICollection<Func<Type, bool>> GetFiltersByTypeParameterAttributes(GenericParameterAttributes genericParameterAttributes)
        {
            var filters = new List<Func<Type, bool>>();

            if (genericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                filters.Add(type => type.IsClass);
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
}