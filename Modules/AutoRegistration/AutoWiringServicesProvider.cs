namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Internals;
    using SimpleInjector;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    [Unregistered]
    public class AutoWiringServicesProvider : IAutoWiringServicesProvider
    {
        private readonly Container _container;
        private readonly ITypeExtensions _typeExtensions;

        /// <summary> .cctor </summary>
        /// <param name="container">Container</param>
        /// <param name="typeExtensions">ITypeExtensions</param>
        public AutoWiringServicesProvider(Container container, ITypeExtensions typeExtensions)
        {
            _container = container;
            _typeExtensions = typeExtensions;
        }

        /// <inheritdoc />
        public IEnumerable<Type> Resolvable()
        {
            return _typeExtensions.AllOurServicesThatContainsDeclarationOfInterface<IResolvable>();
        }

        /// <inheritdoc />
        public IEnumerable<Type> Collections()
        {
            return _typeExtensions.AllOurServicesThatContainsDeclarationOfInterface<ICollectionResolvable>();
        }

        /// <inheritdoc />
        public IEnumerable<Type> Implementations()
        {
            return _container.GetTypesToRegister(_typeExtensions, typeof(IResolvableImplementation));
        }

        /// <inheritdoc />
        public IEnumerable<Type> External()
        {
            return _typeExtensions
                  .AllLoadedTypes()
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && _typeExtensions.IsSubclassOfOpenGeneric(type, typeof(IExternalResolvable<>)))
                  .SelectMany(type => _typeExtensions.GetGenericArgumentsOfOpenGenericAt(type, typeof(IExternalResolvable<>), 0))
                  .Select(_typeExtensions.ExtractGenericTypeDefinition)
                  .Distinct();
        }
    }
}