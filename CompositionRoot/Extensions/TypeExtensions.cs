namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using Abstractions;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly ITypeExtensions _typeExtensions = DependencyContainer.Resolve<ITypeExtensions>();

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