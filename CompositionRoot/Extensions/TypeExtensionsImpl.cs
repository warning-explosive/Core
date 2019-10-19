namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class TypeExtensionsImpl : ITypeExtensions
    {
        private readonly ITypeInfoStorage _typeInfoStorage;

        public TypeExtensionsImpl(ITypeInfoStorage typeInfoStorage)
        {
            _typeInfoStorage = typeInfoStorage;
        }

        /// <inheritdoc />
        public bool IsDerivedFromInterface(Type type, Type @interface)
        {
            return @interface.IsInterface
                   && type.GetInterfaces().Contains(@interface);
        }

        /// <inheritdoc />
        public bool IsImplementationOfOpenGeneric(Type type, Type openGenericBaseType)
        {
            return !type.IsInterface
                   && !openGenericBaseType.IsInterface
                   && openGenericBaseType.IsGenericType
                   && openGenericBaseType.IsGenericTypeDefinition
                   && _typeInfoStorage[type].GenericTypeDefinitions.Contains(openGenericBaseType);
        }

        /// <inheritdoc />
        public bool IsImplementationOfOpenGenericInterface(Type type, Type openGenericInterface)
        {
            return !type.IsInterface
                   && openGenericInterface.IsInterface
                   && openGenericInterface.IsGenericType
                   && openGenericInterface.IsGenericTypeDefinition
                   && _typeInfoStorage[type].GenericInterfaceDefinitions.Contains(openGenericInterface);
        }

        /// <inheritdoc />
        public bool IsContainsInterfaceDeclaration(Type type, Type @interface)
        {
            return _typeInfoStorage[type].DeclaredInterfaces.Contains(@interface);
        }
    }
}