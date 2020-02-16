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
        private static ITypeExtensions _typeExtensions = new TypeExtensionsImpl(new TypeInfoStorage(Array.Empty<Assembly>(), Array.Empty<Assembly>()));

        /// <summary>
        /// Configure type info storage
        /// Must be call only once
        /// </summary>
        /// <param name="assemblies">All loaded assemblies</param>
        /// <param name="rootAssemblies">Root assemblies</param>
        /// <returns>Created instance of ITypeExtensions</returns>
        public static ITypeExtensions Configure(Assembly[] assemblies, Assembly[] rootAssemblies)
        {
            _typeExtensions = new TypeExtensionsImpl(new TypeInfoStorage(assemblies, rootAssemblies));

            return _typeExtensions;
        }

        /// <summary> Order collection by type dependencies (DependencyAttribute) </summary>
        /// <param name="source">Source unordered collection</param>
        /// <param name="accessor">Type accessor</param>
        /// <typeparam name="T">Source items type-argument</typeparam>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered collection</returns>
        public static IOrderedEnumerable<T> OrderByDependencies<T>(this IEnumerable<T> source, Func<T, Type> accessor)
        {
            return _typeExtensions.OrderByDependencies(source, accessor);
        }

        /// <summary>
        /// Get all services (interfaces) that contains TInterface declaration
        /// </summary>
        /// <typeparam name="T">Type-argument</typeparam>
        /// <returns>Result of check</returns>
        public static Type[] AllOurServicesThatContainsDeclarationOfInterface<T>()
        {
            return _typeExtensions.AllOurServicesThatContainsDeclarationOfInterface<T>();
        }

        /// <summary>
        /// Get all types loaded in AppDomain
        /// </summary>
        /// <returns>All types loaded in AppDomain</returns>
        public static Type[] AllLoadedTypes()
        {
            return _typeExtensions.AllLoadedTypes();
        }

        /// <summary>
        /// Get all types located in our assemblies
        /// </summary>
        /// <returns>All types located in our assemblies</returns>
        public static Type[] OurTypes()
        {
            return _typeExtensions.OurTypes();
        }

        /// <summary>
        /// Does type located in our assembly
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Result of check</returns>
        public static bool IsOurType(this Type type)
        {
            return _typeExtensions.IsOurType(type);
        }

        /// <summary>
        /// Get type dependencies from DependencyAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type dependencies</returns>
        public static ICollection<Type> GetDependenciesByAttribute(this Type type)
        {
            return _typeExtensions.GetDependenciesByAttribute(type);
        }

        /// <summary>
        ///  Does type implement Nullable
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <returns>Result of check</returns>
        public static bool IsNullable(this Type type)
        {
            return _typeExtensions.IsNullable(type);
        }

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericAncestor">Open generic ancestor</param>
        /// <returns>Result of check</returns>
        public static bool IsSubclassOfOpenGeneric(this Type type, Type openGenericAncestor)
        {
            return _typeExtensions.IsSubclassOfOpenGeneric(type, openGenericAncestor);
        }

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="interface">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
        public static bool IsContainsInterfaceDeclaration(this Type type, Type @interface)
        {
            return _typeExtensions.IsContainsInterfaceDeclaration(type, @interface);
        }

        /// <summary>
        /// Does type fits for type argument
        /// </summary>
        /// <param name="typeForCheck">Actual type for check</param>
        /// <param name="typeArgument">Type argument</param>
        /// <returns>True - fits / False doesn't fits</returns>
        public static bool FitsForTypeArgument(this Type typeForCheck, Type typeArgument)
        {
            return _typeExtensions.FitsForTypeArgument(typeForCheck, typeArgument);
        }
    }
}