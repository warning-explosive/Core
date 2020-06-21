namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Basics.EqualityComparers;
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

                              Func<InstanceProducer?> getRegistration = () => _container.GetRegistration(t, true);

                              var producer = getRegistration.Try().Catch<ActivationException>().Invoke();

                              return producer == null
                                         ? DependencyInfo.UnregisteredDependencyInfo(t)
                                         : DependencyInfo.RetrieveDependencyGraph(producer, visited, 0);
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

                              Func<object> getInstance = () => _container.GetInstance(closedOrSame);

                              // build graph by invocation
                              getInstance.Try().Catch<ActivationException>().Invoke();

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