namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Services;
    using Basics;

    /// <inheritdoc />
    [SuppressMessage("Analysis", "CR1", Justification = "Registered by hand. See DependencyContainerImpl.")]
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
                  .Where(type => typeof(IResolvable).IsAssignableFrom(type)
                              && type != typeof(IResolvable));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Collections()
        {
            return ExtractGeneric(_typeProvider.OurTypes, typeof(ICollectionResolvable<>));
        }

        /// <inheritdoc />
        public IEnumerable<Type> External()
        {
            return ExtractGeneric(_typeProvider.AllLoadedTypes, typeof(IExternalResolvable<>));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Versions()
        {
            return ExtractGeneric(_typeProvider.OurTypes, typeof(IVersionFor<>));
        }

        /// <inheritdoc />
        public IEnumerable<Type> Decorators()
        {
            return ExtractDecorators(typeof(IDecorator<>));
        }

        /// <inheritdoc />
        public IEnumerable<Type> CollectionDecorators()
        {
            return ExtractDecorators(typeof(ICollectionDecorator<>));
        }

        private IEnumerable<Type> ExtractDecorators(Type decorator)
        {
            return _typeProvider
                  .OurTypes
                  .Where(t => t.IsClass
                           && !t.IsInterface
                           && !t.IsAbstract
                           && t.IsSubclassOfOpenGeneric(decorator));
        }

        private static IEnumerable<Type> ExtractGeneric(IEnumerable<Type> source, Type openGenericService)
        {
            return source
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && type.IsSubclassOfOpenGeneric(openGenericService))
                  .SelectMany(type => type.ExtractGenericArgumentsAt(openGenericService, 0))
                  .Where(type => !type.IsGenericParameter)
                  .Select(type => type.GenericTypeDefinitionOrSelf())
                  .Distinct();
        }
    }
}