namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System;

    internal interface ITypeExtensions : IResolvable
    {
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