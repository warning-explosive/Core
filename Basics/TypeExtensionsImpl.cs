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
            int SortFunc(T item)
            {
                var type = accessor(item);
                var dependencies = GetDependencies(type).AsEnumerable();

                var depth = 0;

                while (dependencies.Any())
                {
                    if (dependencies.Contains(type))
                    {
                        throw new InvalidOperationException($"{type} has cycle dependency");
                    }

                    ++depth;

                    dependencies = dependencies.SelectMany(GetDependencies);
                }

                return depth;
            }

            return source.OrderBy(SortFunc);
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
            return _typeInfoStorage.ContainsKey(ExtractGenericTypeDefinition(type));
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
        public bool IsSubclassOfOpenGeneric(Type type, Type openGenericAncestor)
        {
            if (!openGenericAncestor.IsGenericTypeDefinition)
            {
                return false;
            }

            return _typeInfoStorage[type].GenericTypeDefinitions.Contains(openGenericAncestor)
                || _typeInfoStorage[type].GenericInterfaceDefinitions.Contains(openGenericAncestor);
        }

        /// <inheritdoc />
        public bool IsContainsInterfaceDeclaration(Type type, Type i)
        {
            if (_typeInfoStorage[type].DeclaredInterfaces.Contains(i))
            {
                return true;
            }

            // generic
            var genericTypeDefinition = ExtractGenericTypeDefinition(type);
            var genericInterfaceDefinition = ExtractGenericTypeDefinition(i);

            return _typeInfoStorage[genericTypeDefinition].DeclaredInterfaces
                                                          .Select(z => z.GUID)
                                                          .Contains(genericInterfaceDefinition.GUID)
                && type.GenericTypeArguments.SequenceEqual(i.GenericTypeArguments);
        }

        private static Type ExtractGenericTypeDefinition(Type type)
        {
            return type.IsGenericType
                       ? type.GetGenericTypeDefinition()
                       : type;
        }
    }
}