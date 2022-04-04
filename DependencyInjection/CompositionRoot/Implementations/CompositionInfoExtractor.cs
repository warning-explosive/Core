namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions;
    using Api.Abstractions.CompositionInfo;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SimpleInjector;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton)]
    internal class CompositionInfoExtractor : ICompositionInfoExtractor
    {
        private readonly Container _container;
        private readonly IGenericTypeProvider _provider;
        private readonly IAutoRegistrationServicesProvider _autoRegistrationServiceProvider;

        /// <summary> .ctor </summary>
        /// <param name="container">Container</param>
        /// <param name="provider">IGenericArgumentsReceiver</param>
        /// <param name="autoRegistrationServiceProvider">IServiceProvider</param>
        public CompositionInfoExtractor(Container container,
                                        IGenericTypeProvider provider,
                                        IAutoRegistrationServicesProvider autoRegistrationServiceProvider)
        {
            _container = container;
            _provider = provider;
            _autoRegistrationServiceProvider = autoRegistrationServiceProvider;
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
            return _autoRegistrationServiceProvider.Resolvable().Select(CloseOpenGeneric)
                .Concat(_autoRegistrationServiceProvider.External().Select(CloseOpenGeneric))
                .Concat(_autoRegistrationServiceProvider.Collections().Select(CloseOpenGenericCollection));
        }

        private Type CloseOpenGeneric(Type type)
        {
            var closedOrSame = _provider.CloseByConstraints(type, ctx => ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault());

            // build graph by invocation
            Func<object?> getInstance = () => _container.GetInstance(closedOrSame);

            ExecutionExtensions
                .Try(getInstance)
                .Catch<ActivationException>()
                .Invoke(_ => default);

            return closedOrSame;
        }

        private Type CloseOpenGenericCollection(Type type)
        {
            var closedOrSame = _provider.CloseByConstraints(type, ctx => ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault());

            // build graph by invocation
            Func<IEnumerable<object>?> getAllInstances = () => _container.GetAllInstances(closedOrSame);

            ExecutionExtensions
                .Try(getAllInstances)
                .Catch<ActivationException>()
                .Invoke(_ => default);

            return typeof(IEnumerable<>).MakeGenericType(closedOrSame);
        }
    }
}