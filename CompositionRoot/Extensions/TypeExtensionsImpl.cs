namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class TypeExtensionsImpl : ITypeExtensions
    {
        private readonly ITypeInfoStorage _typeInfoStorage;

        /// <summary> .ctor </summary>
        /// <param name="typeInfoStorage">ITypeInfoStorage</param>
        public TypeExtensionsImpl(ITypeInfoStorage typeInfoStorage)
        {
            _typeInfoStorage = typeInfoStorage;
        }

        /// <inheritdoc />
        public bool IsNullable(Type type)
        {
            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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
            var condition = _typeInfoStorage[type].DeclaredInterfaces.Contains(@interface);

            if (condition)
            {
                return true;
            }

            return TryGetGenericTypeDefinition(type, out var genericTypeDefinition)
                   && TryGetGenericTypeDefinition(@interface, out var genericInterfaceDefinition)
                   && _typeInfoStorage[genericTypeDefinition].DeclaredInterfaces.Select(z => z.GUID).Contains(genericInterfaceDefinition.GUID)
                   && type.GetGenericArguments().SequenceEqual(@interface.GenericTypeArguments);
        }

        private static bool TryGetGenericTypeDefinition(Type t, out Type genericTypeDefinition)
        {
            if (t.IsGenericType)
            {
                genericTypeDefinition = t.GetGenericTypeDefinition();
                return true;
            }

            genericTypeDefinition = t;
            return false;
        }
    }
}