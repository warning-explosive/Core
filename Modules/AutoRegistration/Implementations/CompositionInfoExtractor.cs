namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Internals;
    using SimpleInjector;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton)]
    internal class CompositionInfoExtractor : ICompositionInfoExtractor
    {
        private readonly Container _container;
        private readonly IGenericTypeProvider _provider;
        private readonly IAutoWiringServicesProvider _autoWiringServiceProvider;

        /// <summary> .ctor </summary>
        /// <param name="container">Container</param>
        /// <param name="provider">IGenericArgumentsReceiver</param>
        /// <param name="autoWiringServiceProvider">IServiceProvider</param>
        public CompositionInfoExtractor(Container container,
                                        IGenericTypeProvider provider,
                                        IAutoWiringServicesProvider autoWiringServiceProvider)
        {
            _container = container;
            _provider = provider;
            _autoWiringServiceProvider = autoWiringServiceProvider;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IDependencyInfo> GetCompositionInfo(bool activeMode)
        {
            if (activeMode)
            {
                return GetClosedServices()
                      .Select(t =>
                              {
                                  var producer = _container.GetRegistration(t, false);

                                  return producer == null
                                      ? DependencyInfo.UnregisteredDependencyInfo(t)
                                      : DependencyInfo.RetrieveDependencyGraph(producer);
                              })
                      .ToArray();
            }

            return _container.GetCurrentRegistrations()
                             .Select(DependencyInfo.RetrieveDependencyGraph)
                             .ToArray();
        }

        private IEnumerable<Type> GetClosedServices()
        {
            return _autoWiringServiceProvider.Resolvable().Select(CloseOpenGeneric)
                                             .Concat(_autoWiringServiceProvider.External().Select(CloseOpenGeneric))
                                             .Concat(_autoWiringServiceProvider.Collections().Select(CloseOpenGenericCollection));
        }

        private Type CloseOpenGeneric(Type type)
        {
            var closedOrSame = _provider.CloseByConstraints(type, ctx => ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault());

            // build graph by invocation
            Func<object> getInstance = () => _container.GetInstance(closedOrSame);
            getInstance.Try().Catch<ActivationException>().Invoke();

            return closedOrSame;
        }

        private Type CloseOpenGenericCollection(Type type)
        {
            var closedOrSame = _provider.CloseByConstraints(type, ctx => ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault());

            // build graph by invocation
            Func<IEnumerable<object>> getAllInstances = () => _container.GetAllInstances(closedOrSame);
            getAllInstances.Try().Catch<ActivationException>().Invoke();

            return typeof(IEnumerable<>).MakeGenericType(closedOrSame);
        }
    }
}