namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using SimpleInjector;

    /// <inheritdoc />
    [ManualRegistration]
    internal class AutoWiringServicesProvider : IAutoWiringServicesProvider
    {
        private readonly ITypeProvider _typeProvider;

        /// <summary> .cctor </summary>
        /// <param name="typeProvider">ITypeProvider</param>
        public AutoWiringServicesProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        /// <inheritdoc />
        public IEnumerable<Type> Resolvable()
        {
            return _typeProvider
                  .OurTypes
                  .Where(type => type.IsInterface
                              && typeof(IResolvable).IsAssignableFrom(type)
                              && type != typeof(IResolvable));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Collections()
        {
            return _typeProvider
                  .OurTypes
                  .Where(type => type.IsInterface
                              && typeof(ICollectionResolvable).IsAssignableFrom(type)
                              && type != typeof(ICollectionResolvable));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Implementations()
        {
            return _typeProvider
                  .OurTypes
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && typeof(IResolvable).IsAssignableFrom(type));
        }

        /// <inheritdoc />
        public IEnumerable<Type> External()
        {
            return _typeProvider
                  .AllLoadedTypes
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && type.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>)))
                  .SelectMany(type => type.ExtractGenericArgumentsAt(typeof(IExternalResolvable<>), 0))
                  .Where(type => !type.IsGenericParameter)
                  .Select(type => type.GenericTypeDefinitionOrSelf())
                  .Distinct();
        }

        /// <inheritdoc />
        public IEnumerable<Type> Versions()
        {
            return _typeProvider
                  .OurTypes
                  .Where(type => type.IsClass
                           && !type.IsAbstract
                           && type.IsSubclassOfOpenGeneric(typeof(IVersionFor<>)))
                  .SelectMany(type => type.ExtractGenericArgumentsAt(typeof(IVersionFor<>), 0))
                  .Where(type => !type.IsGenericParameter)
                  .Distinct();
        }

        /// <inheritdoc />
        public IEnumerable<Type> Decorators()
        {
            return _typeProvider
                  .OurTypes
                  .Where(t => t.IsClass
                           && !t.IsInterface
                           && !t.IsAbstract
                           && t.IsSubclassOfOpenGeneric(typeof(IDecorator<>)));
        }

        /// <inheritdoc />
        public IEnumerable<Type> CollectionDecorators()
        {
            return _typeProvider
                  .OurTypes
                  .Where(t => t.IsClass
                           && !t.IsInterface
                           && !t.IsAbstract
                           && t.IsSubclassOfOpenGeneric(typeof(ICollectionDecorator<>)));
        }
    }
}