namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Reflection;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static class TypeExtensions
    {
        private static ITypeExtensions _typeExtensions = new TypeExtensionsImpl(new TypeInfoStorage(Array.Empty<Assembly>()));

        /// <summary>
        /// Set instance
        /// </summary>
        /// <param name="assemblies">assemblies</param>
        /// <returns>Created instance of ITypeExtensions</returns>
        public static ITypeExtensions SetInstance(Assembly[] assemblies)
        {
            _typeExtensions = new TypeExtensionsImpl(new TypeInfoStorage(assemblies));

            return _typeExtensions;
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
        /// Get type order from OrderAttribute
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type order</returns>
        public static uint? GetOrder(this Type type)
        {
            return _typeExtensions.GetOrder(type);
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
        /// Does type derived from interface
        /// </summary>
        /// <param name="type">Type-implementor</param>
        /// <param name="interface">interface</param>
        /// <returns>Result of check</returns>
        public static bool IsDerivedFromInterface(this Type type, Type @interface)
        {
            return _typeExtensions.IsDerivedFromInterface(type, @interface);
        }

        /// <summary>
        /// Does type derived from open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericBaseType">Open generic base type</param>
        /// <returns>Result of check</returns>
        public static bool IsImplementationOfOpenGeneric(this Type type, Type openGenericBaseType)
        {
            return _typeExtensions.IsImplementationOfOpenGeneric(type, openGenericBaseType);
        }
        
        /// <summary>
        /// Does type implement open-generic interface
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericInterface">Open generic interface</param>
        /// <returns>Result of check</returns>
        public static bool IsImplementationOfOpenGenericInterface(this Type type, Type openGenericInterface)
        {
            return _typeExtensions.IsImplementationOfOpenGenericInterface(type, openGenericInterface);
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
    }
}