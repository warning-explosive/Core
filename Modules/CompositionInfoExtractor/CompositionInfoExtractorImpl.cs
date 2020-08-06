namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
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
        private readonly IAutoWiringServicesProvider _autoWiringServiceProvider;

        /// <summary> .ctor </summary>
        /// <param name="container">Container</param>
        /// <param name="receiver">IGenericArgumentsReceiver</param>
        /// <param name="autoWiringServiceProvider">IServiceProvider</param>
        public CompositionInfoExtractorImpl(Container container,
                                            IGenericArgumentsReceiver receiver,
                                            IAutoWiringServicesProvider autoWiringServiceProvider)
        {
            _container = container;
            _receiver = receiver;
            _autoWiringServiceProvider = autoWiringServiceProvider;
        }

        /// <inheritdoc />
        public DependencyInfo[] GetCompositionInfo(bool activeMode)
        {
            if (activeMode)
            {
                return GetClosedServices()
                      .Select(t =>
                              {
                                  var producer = _container.GetRegistration(t, false);

                                  if (producer == null)
                                  {
                                      return DependencyInfo.UnregisteredDependencyInfo(t);
                                  }

                                  var visited = new Dictionary<InstanceProducer, DependencyInfo>(new ReferenceEqualityComparer<InstanceProducer>());
                                  return DependencyInfo.RetrieveDependencyGraph(producer, visited, 0);
                              })
                      .ToArray();
            }

            return _container.GetCurrentRegistrations()
                             .Select(producer =>
                                     {
                                         var visited = new Dictionary<InstanceProducer, DependencyInfo>(new ReferenceEqualityComparer<InstanceProducer>());
                                         return DependencyInfo.RetrieveDependencyGraph(producer, visited, 0);
                                     })
                             .ToArray();
        }

        private IEnumerable<Type> GetClosedServices()
        {
            return _autoWiringServiceProvider.Resolvable().Select(CloseOpenGeneric)
                                             .Concat(_autoWiringServiceProvider.Implementations().Select(CloseOpenGeneric))
                                             .Concat(_autoWiringServiceProvider.External().Select(CloseOpenGeneric))
                                             .Concat(_autoWiringServiceProvider.Collections().Select(CloseOpenGenericCollection));
        }

        private Type CloseOpenGeneric(Type type)
        {
            var closedOrSame = _receiver.CloseByConstraints(type);

            // build graph by invocation
            Func<object> getInstance = () => _container.GetInstance(closedOrSame);
            getInstance.Try().Catch<ActivationException>().Invoke();

            return closedOrSame;
        }

        private Type CloseOpenGenericCollection(Type type)
        {
            var closedOrSame = _receiver.CloseByConstraints(type);

            // build graph by invocation
            Func<IEnumerable<object>> getAllInstances = () => _container.GetAllInstances(closedOrSame);
            getAllInstances.Try().Catch<ActivationException>().Invoke();

            return typeof(IEnumerable<>).MakeGenericType(closedOrSame);
        }
    }
}