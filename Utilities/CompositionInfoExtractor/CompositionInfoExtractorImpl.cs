namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Basics.EqualityComparers;
    using CompositionRoot;
    using CompositionRoot.Abstractions;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using SimpleInjector;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CompositionInfoExtractorImpl : ICompositionInfoExtractor
    {
        private readonly Container _container;
        private readonly IGenericArgumentsReceiver _receiver;

        /// <summary> .ctor </summary>
        /// <param name="container">Container</param>
        /// <param name="receiver">IGenericArgumentsReceiver</param>
        public CompositionInfoExtractorImpl(Container container,
                                            IGenericArgumentsReceiver receiver)
        {
            _container = container;
            _receiver = receiver;
        }

        /// <inheritdoc />
        public DependencyInfo[] GetCompositionInfo()
        {
            return GetClosedServices()
                  .Select(t =>
                          {
                              var visited = new Dictionary<InstanceProducer, DependencyInfo>(new ReferenceEqualityComparer<InstanceProducer>());

                              return DependencyInfo.RetrieveDependencyGraph(_container.GetRegistration(t, true).TryExtractFromNullable(),
                                                                            visited,
                                                                            0);
                          })
                  .ToArray();
        }

        private IEnumerable<Type> GetClosedServices()
        {
            return TypeExtensions
                  .AllOurServicesThatContainsDeclarationOfInterface<IResolvable>()
                  .Select(t =>
                          {
                              var closedOrSame = _receiver.CloseByConstraints(t);

                              _container.GetInstance(closedOrSame);

                              return closedOrSame;
                          })
                  .Concat(TypeExtensions
                         .AllOurServicesThatContainsDeclarationOfInterface<ICollectionResolvable>()
                         .Select(t =>
                                 {
                                     var closedOrSame = _receiver.CloseByConstraints(t);

                                     _container.GetAllInstances(closedOrSame);

                                     return typeof(IEnumerable<>).MakeGenericType(closedOrSame);
                                 }));
        }
    }
}