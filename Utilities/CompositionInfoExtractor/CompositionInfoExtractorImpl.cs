namespace SpaceEngineers.Core.Utilities.CompositionInfoExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompositionRoot;
    using CompositionRoot.Abstractions;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using CompositionRoot.Extensions;
    using SimpleInjector;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CompositionInfoExtractorImpl : ICompositionInfoExtractor
    {
        private readonly Container _container;
        private readonly ITypeExtensions _typeExtensions;
        private readonly IGenericArgumentsInferer _genericArgumentsInferer;

        /// <summary> .ctor </summary>
        /// <param name="container">Container</param>
        /// <param name="typeExtensions">ITypeExtensions</param>
        /// <param name="genericArgumentsInferer"></param>
        public CompositionInfoExtractorImpl(Container container,
                                            ITypeExtensions typeExtensions,
                                            IGenericArgumentsInferer genericArgumentsInferer)
        {
            _container = container;
            _typeExtensions = typeExtensions;
            _genericArgumentsInferer = genericArgumentsInferer;
        }

        /// <inheritdoc />
        public DependencyInfo[] GetCompositionInfo()
        {
            return GetClosedServices()
                  .Select(t =>
                          {
                              var visited = new Dictionary<InstanceProducer, DependencyInfo>(new ReferenceEqualityComparer<InstanceProducer>());

                              return DependencyInfo.RetrieveDependencyGraph(_container.GetRegistration(t, true).ThrowIfNull(),
                                                                            visited,
                                                                            0);
                          })
                  .ToArray();
        }

        private IEnumerable<Type> GetClosedServices()
        {
            return _typeExtensions
                  .AllOurServicesThatContainsDeclarationOfInterface<IResolvable>()
                  .Select(t =>
                          {
                              var closedOrSame = _genericArgumentsInferer.CloseByConstraints(t);

                              _container.GetInstance(closedOrSame);

                              return closedOrSame;
                          })
                  .Concat(_typeExtensions
                         .AllOurServicesThatContainsDeclarationOfInterface<ICollectionResolvable>()
                         .Select(t =>
                                 {
                                     var closedOrSame = _genericArgumentsInferer.CloseByConstraints(t);

                                     _container.GetAllInstances(closedOrSame);

                                     return typeof(IEnumerable<>).MakeGenericType(closedOrSame);
                                 }));
        }
    }
}