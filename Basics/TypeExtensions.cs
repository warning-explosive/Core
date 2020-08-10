namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static class TypeExtensions
    {
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
            if (typeArgumentAt < 0 || typeArgumentAt >= openGeneric.GetGenericArguments().Length)
            {
                throw new ArgumentException("Must be in bounds of generic arguments count", nameof(typeArgumentAt));
            }

            if (!openGeneric.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Must be GenericTypeDefinition", nameof(openGeneric));
            }

            return !IsSubclassOfOpenGeneric(source, openGeneric)
                       ? Enumerable.Empty<Type>()
                       : new[] { source }
                        .Concat(TypeInfoStorage.Get(source).BaseTypes)
                        .Concat(source.GetInterfaces())
                        .Where(type => type.GenericTypeDefinitionOrSelf() == openGeneric)
                        .Select(type => type.GetGenericArguments()[typeArgumentAt]);
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
        /// Does type fits for type argument
        /// </summary>
        /// <param name="typeForCheck">Actual type for check</param>
        /// <param name="typeArgument">Type argument</param>
        /// <returns>True - fits / False doesn't fits</returns>
        public static bool FitsForTypeArgument(this Type typeForCheck, Type typeArgument)
        {
            if (!typeArgument.IsGenericParameter)
            {
                return typeArgument.IsAssignableFrom(typeForCheck);
            }

            bool CheckConstraint(Type constraint) =>
                constraint.IsGenericType
                    ? IsSubclassOfOpenGeneric(typeForCheck, constraint.GetGenericTypeDefinition())
                    : constraint.IsAssignableFrom(typeForCheck);

            var byConstraints = typeArgument.GetGenericParameterConstraints()
                                            .All(CheckConstraint);

            var filters = GetFiltersByTypeParameterAttributes(typeArgument.GenericParameterAttributes);
            var byGenericParameterAttributes = filters.All(filter => filter(typeForCheck));

            return byConstraints && byGenericParameterAttributes;
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