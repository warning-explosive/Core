namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Extensions for operations with System.Type class
    /// </summary>
    public interface ITypeExtensions
    {
        /// <summary>
        /// Get all services (interfaces) that contains TInterface declaration
        /// </summary>
        /// <typeparam name="T">Type-argument</typeparam>
        /// <returns>Result of check</returns>
        Type[] AllOurServicesThatContainsDeclarationOfInterface<T>();

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
        /// Get type order from OrderAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type order</returns>
        uint? GetOrder(Type type);
        
        /// <summary>
        /// Does type implement Nullable
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <returns>Result of check</returns>
        bool IsNullable(Type type);
        
        /// <summary>
        /// Does type derived from interface
        /// </summary>
        /// <param name="type">Type-implementor</param>
        /// <param name="interface">interface</param>
        /// <returns>Result of check</returns>
        bool IsDerivedFromInterface(Type type, Type @interface);

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericBaseType">Open generic base type</param>
        /// <returns>Result of check</returns>
        bool IsImplementationOfOpenGeneric(Type type, Type openGenericBaseType);

        /// <summary>
        /// Does type implement open-generic interface
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericInterface">Open generic interface</param>
        /// <returns>Result of check</returns>
        bool IsImplementationOfOpenGenericInterface(Type type, Type openGenericInterface);

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="interface">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
        bool IsContainsInterfaceDeclaration(Type type, Type @interface);
    }
}