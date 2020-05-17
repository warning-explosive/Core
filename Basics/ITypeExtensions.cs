namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions for operations with System.Type class
    /// </summary>
    public interface ITypeExtensions
    {
        /// <summary> Order collection by type dependencies (DependencyAttribute) </summary>
        /// <param name="source">Source unordered collection</param>
        /// <param name="accessor">Type accessor</param>
        /// <typeparam name="T">Source items type-argument</typeparam>
        /// <exception cref="InvalidOperationException">Source type has cycle dependency</exception>
        /// <returns>Ordered collection</returns>
        IOrderedEnumerable<T> OrderByDependencies<T>(IEnumerable<T> source, Func<T, Type> accessor);

        /// <summary>
        /// Get all services (interfaces) that contains TInterface declaration
        /// </summary>
        /// <typeparam name="TInterface">Interface type-argument</typeparam>
        /// <returns>Result of check</returns>
        Type[] AllOurServicesThatContainsDeclarationOfInterface<TInterface>()
            where TInterface : class;

        /// <summary>
        /// Get all types loaded in AppDomain
        /// </summary>
        /// <returns>All types loaded in AppDomain</returns>
        Type[] AllLoadedTypes();

        /// <summary>
        /// Get all types located in our assemblies
        /// </summary>
        /// <returns>All types located in our assemblies</returns>
        Type[] OurTypes();

        /// <summary>
        /// Get all our assemblies (with dependency on CompositionRoot assembly)
        /// </summary>
        /// <returns>All our assemblies</returns>
        Assembly[] OurAssemblies();

        /// <summary>
        /// Does type located in our assembly
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Result of check</returns>
        bool IsOurType(Type type);

        /// <summary>
        /// Get type dependencies from DependencyAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type dependencies</returns>
        Type[] GetDependenciesByAttribute(Type type);

        /// <summary>
        /// Does type implement Nullable
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <returns>Result of check</returns>
        bool IsNullable(Type type);

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericAncestor">Open generic ancestor</param>
        /// <returns>Result of check</returns>
        bool IsSubclassOfOpenGeneric(Type type, Type openGenericAncestor);

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="i">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
        bool IsContainsInterfaceDeclaration(Type type, Type i);

        /// <summary>
        /// Does type fits for type argument
        /// </summary>
        /// <param name="typeForCheck">Actual type for check</param>
        /// <param name="typeArgument">Type argument</param>
        /// <returns>True - fits / False doesn't fits</returns>
        bool FitsForTypeArgument(Type typeForCheck, Type typeArgument);

        /// <summary>
        /// Extract type-arguments from derived class of open-generic type at specified index
        /// Distinct, might be several implementations with different type-arguments
        /// </summary>
        /// <param name="derived">Derived from open-generic</param>
        /// <param name="openGeneric">Open-generic which derived</param>
        /// <param name="typeArgumentAt">Index of type-argument</param>
        /// <returns>Collection of type arguments at specified index.</returns>
        IEnumerable<Type> GetGenericArgumentsOfOpenGenericAt(Type derived,
                                                             Type openGeneric,
                                                             int typeArgumentAt = 0);

        /// <summary>
        /// Extract GenericTypeDefinition or return argument type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>GenericTypeDefinition or argument type</returns>
        Type ExtractGenericTypeDefinition(Type type);
    }
}