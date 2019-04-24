namespace SpaceEngineers.Core.Utilities.Extensions
{
    using System;
    using System.Linq;

    /// <summary>
    /// System.Type extensions methods
    /// </summary>
    public static partial class TypeExtensions
    {
        /// <summary>
        /// Does type derived from interface
        /// </summary>
        /// <param name="type">Type-implementor</param>
        /// <param name="i">interface</param>
        /// <returns>Result of check</returns>
        public static bool IsDerivedFromInterface(this Type type, Type i)
        {
            return i.IsInterface
                   && type.GetInterfaces()
                          .Contains(i);
        }

        /// <summary>
        /// Does type implement open-generic base type
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericBaseType">Open generic base type</param>
        /// <returns>Result of check</returns>
        public static bool IsImplementationOfOpenGeneric(this Type type, Type openGenericBaseType)
        {
            var genericTypeDefinitions = TypeInfoStorage.Collection
                                                        .GetOrAdd(type, t => new TypeInfoStorage.TypeInfo(t))
                                                        .GenericTypeDefinitions;

            return !openGenericBaseType.IsInterface
                   && openGenericBaseType.IsGenericType
                   && openGenericBaseType.IsGenericTypeDefinition
                   && genericTypeDefinitions.Contains(openGenericBaseType);
        }
        
        /// <summary>
        /// Does type implement open-generic interface
        /// </summary>
        /// <param name="type">Type for check</param>
        /// <param name="openGenericInterface">Open generic interface</param>
        /// <returns>Result of check</returns>
        public static bool IsImplementationOfOpenGenericInterface(this Type type, Type openGenericInterface)
        {
            var genericInterfaceDefinitions = TypeInfoStorage.Collection
                                                             .GetOrAdd(type, t => new TypeInfoStorage.TypeInfo(t))
                                                             .GenericInterfaceDefinitions;
            
            return openGenericInterface.IsInterface
                   && openGenericInterface.IsGenericType
                   && openGenericInterface.IsGenericTypeDefinition
                   && genericInterfaceDefinitions.Contains(openGenericInterface);
        }

        /// <summary>
        /// Does type contains interface declaration
        /// </summary>
        /// <param name="i">Interface for check</param>
        /// <param name="type">Type-candidate for interface declaration</param>
        /// <returns>Result of check</returns>
        public static bool IsContainsInterfaceDeclaration(this Type i, Type type)
        {
            if (!i.IsInterface)
            {
                return false;
            }

            var declaredInterfaces = TypeInfoStorage.Collection
                                                    .GetOrAdd(type, t => new TypeInfoStorage.TypeInfo(t))
                                                    .DeclaredInterfaces;

            return declaredInterfaces.Contains(i);
        }
    }
}