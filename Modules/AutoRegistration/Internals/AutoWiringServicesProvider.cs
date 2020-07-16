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
    internal class AutoWiringServicesProvider : IAutoWiringServicesProvider
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
            return typeof(IResolvableImplementation).GetTypesToRegister(_container, _typeExtensions);
        }

        /// <inheritdoc />
        public IEnumerable<Type> External()
        {
            return _typeExtensions
                  .AllLoadedTypes()
                  .Where(type => type.IsClass
                              && !type.IsAbstract
                              && type.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>)))
                  .SelectMany(type => type.GetGenericArgumentsOfOpenGenericAt(typeof(IExternalResolvable<>), 0))
                  .Select(_typeExtensions.ExtractGenericTypeDefinition)
                  .Distinct();
        }
    }
}