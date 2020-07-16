namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly ITypeExtensions EmptyDefault = new TypeExtensionsImpl(new TypeInfoStorage(Array.Empty<Assembly>(), Array.Empty<Assembly>()));

        private static Func<ITypeExtensions>? _typeExtensionsFactory;

        private static ITypeExtensions TypeExtensionsService => _typeExtensionsFactory?.Invoke() ?? EmptyDefault;

        /// <summary>
        /// Build type extensions without side effects
        /// </summary>
        /// <param name="assemblies">All loaded assemblies</param>
        /// <param name="rootAssemblies">Root assemblies</param>
        /// <returns>Created instance of ITypeExtensions</returns>
        public static ITypeExtensions Build(Assembly[] assemblies, Assembly[] rootAssemblies)
        {
            return new TypeExtensionsImpl(new TypeInfoStorage(assemblies, rootAssemblies));
        }

        /// <summary>
        /// Configure type extensions class
        /// Must be call only once
        /// </summary>
        /// <param name="typeExtensionsFactory">ITypeExtensions factory</param>
        public static void Configure(Func<ITypeExtensions> typeExtensionsFactory)
        {
            _typeExtensionsFactory = typeExtensionsFactory;
        }

        /// <summary> Order collection by type dependencies (DependencyAttribute) </summary>
        /// <param name="source">Source unordered collection</param>
        /// <param name="accessor">Type accessor</param>
        /// <typeparam name="T">Source items type-argument</typeparam>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered collection</returns>
        public static IOrderedEnumerable<T> OrderByDependencies<T>(this IEnumerable<T> source, Func<T, Type> accessor)
        {
            return TypeExtensionsService.OrderByDependencies(source, accessor);
        }

        /// <summary>
        /// Does type located in our assembly
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Result of check</returns>
        public static bool IsOurType(this Type type)
        {
            return TypeExtensionsService.IsOurType(type);
        }

        /// <summary>
        /// Get type dependencies from DependencyAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type dependencies</returns>
        public static ICollection<Type> GetDependenciesByAttribute(this Type type)
        {
            return TypeExtensionsService.GetDependenciesByAttribute(type);
        }

        /// <summary>
        ///  Does type implement Nullable
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <returns>Result of check</returns>
        public static bool IsNullable(this Type type)
        {
            return TypeExtensionsService.IsNullable(type);
        }

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericAncestor">Open generic ancestor</param>
        /// <returns>Result of check</returns>
        public static bool IsSubclassOfOpenGeneric(this Type type, Type openGenericAncestor)
        {
            return TypeExtensionsService.IsSubclassOfOpenGeneric(type, openGenericAncestor);
        }

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="interface">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
        public static bool IsContainsInterfaceDeclaration(this Type type, Type @interface)
        {
            return TypeExtensionsService.IsContainsInterfaceDeclaration(type, @interface);
        }

        /// <summary>
        /// Does type fits for type argument
        /// </summary>
        /// <param name="typeForCheck">Actual type for check</param>
        /// <param name="typeArgument">Type argument</param>
        /// <returns>True - fits / False doesn't fits</returns>
        public static bool FitsForTypeArgument(this Type typeForCheck, Type typeArgument)
        {
            return TypeExtensionsService.FitsForTypeArgument(typeForCheck, typeArgument);
        }

        /// <summary>
        /// Extract type-arguments from derived class of open-generic type at specified index
        /// Distinct, might be several implementations with different type-arguments
        /// </summary>
        /// <param name="derived">Derived from open-generic</param>
        /// <param name="openGeneric">Open-generic which derived</param>
        /// <param name="typeArgumentAt">Index of type-argument</param>
        /// <returns>Collection of type arguments at specified index.</returns>
        public static IEnumerable<Type> GetGenericArgumentsOfOpenGenericAt(this Type derived,
                                                                           Type openGeneric,
                                                                           int typeArgumentAt = 0)
        {
            return TypeExtensionsService.GetGenericArgumentsOfOpenGenericAt(derived, openGeneric, typeArgumentAt);
        }

        /// <summary>
        /// Extract GenericTypeDefinition or return argument type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>GenericTypeDefinition or argument type</returns>
        public static Type ExtractGenericTypeDefinition(this Type type)
        {
            return TypeExtensionsService.ExtractGenericTypeDefinition(type);
        }
    }
}