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

        /// <inheritdoc />
        public bool FitsForTypeArgument(Type typeForCheck, Type typeArgument)
        {
            if (!typeArgument.IsGenericParameter)
            {
                return typeArgument.IsAssignableFrom(typeForCheck);
            }

            bool CheckConstraint(Type constraint) =>
                constraint.IsGenericType
                    ? typeForCheck.IsSubclassOfOpenGeneric(constraint.GetGenericTypeDefinition())
                    : constraint.IsAssignableFrom(typeForCheck);

            var byConstraints = typeArgument.GetGenericParameterConstraints()
                                            .All(CheckConstraint);

            var filters = GetFiltersByTypeParameterAttributes(typeArgument.GenericParameterAttributes);
            var byFilters = filters.All(filter => filter(typeForCheck));

            return byConstraints && byFilters;
        }

        private static ICollection<Func<Type, bool>> GetFiltersByTypeParameterAttributes(GenericParameterAttributes genericParameterAttributes)
        {
            var filters = new List<Func<Type, bool>>();

            if ((genericParameterAttributes & GenericParameterAttributes.None) != 0)
            {
                filters.Add(type => true);

                return filters;
            }

            if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                filters.Add(type => type.IsClass);
            }

            if ((genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                filters.Add(type => type.IsValueType);
            }

            if ((genericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                filters.Add(type =>
                            {
                                if (type.IsEnum)
                                {
                                    return true;
                                }

                                var ctor = type.GetConstructor(Array.Empty<Type>());

                                return ctor != null;
                            });
            }

            return filters;
        }

        private static Type ExtractGenericTypeDefinition(Type type)
        {
            return type.IsGenericType
                       ? type.GetGenericTypeDefinition()
                       : type;
        }
    }
}