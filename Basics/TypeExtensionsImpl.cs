namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <inheritdoc />
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
        public IOrderedEnumerable<T> OrderByDependencies<T>(IEnumerable<T> source, Func<T, Type> accessor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Type[] AllOurServicesThatContainsDeclarationOfInterface<TInterface>()
        {
            var type = typeof(TInterface);

            if (!type.IsInterface)
            {
                throw new ArgumentException(typeof(TInterface).FullName);
            }

            return _typeInfoStorage
                  .OurTypes
                  .Where(t => t.IsInterface
                              && IsContainsInterfaceDeclaration(t, typeof(TInterface)))
                  .ToArray();
        }

        /// <inheritdoc />
        public Type[] AllLoadedTypes()
        {
            return _typeInfoStorage.AllLoadedTypes;
        }

        /// <inheritdoc />
        public Type[] OurTypes()
        {
            return _typeInfoStorage.OurTypes;
        }

        /// <inheritdoc />
        public Assembly[] OurAssemblies()
        {
            return _typeInfoStorage.OurAssemblies;
        }

        /// <inheritdoc />
        public bool IsOurType(Type type)
        {
            return _typeInfoStorage.ContainsKey(type)
                   || (type.IsGenericType
                       && _typeInfoStorage.ContainsKey(type.GetGenericTypeDefinition()));
        }

        /// <inheritdoc />
        public Type[] GetDependencies(Type type)
        {
            return _typeInfoStorage[type].Dependencies;
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

            // generic
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