namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Async;
    using Basics.Exceptions;
    using Core.GenericHost;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Internals;

    [ManualRegistration]
    internal class InMemoryIntegrationTransport : IIntegrationTransport, IAsyncDisposable
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, IReadOnlyCollection<IGenericEndpoint>>> TopologyMap
            = new ConcurrentDictionary<Type, IDictionary<string, IReadOnlyCollection<IGenericEndpoint>>>();

        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;

        private readonly AsyncManualResetEvent _manualResetEvent;
        private readonly IAsyncDisposable _rootScope;

        public InMemoryIntegrationTransport(
            IDependencyContainer dependencyContainer,
            IEndpointInstanceSelectionBehavior selectionBehavior)
        {
            DependencyContainer = dependencyContainer;
            _selectionBehavior = selectionBehavior;

            _manualResetEvent = new AsyncManualResetEvent(false);
            _rootScope = dependencyContainer.OpenScopeAsync();
        }

        /// <inheritdoc />
        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        /// <inheritdoc />
        public IDependencyContainer DependencyContainer { get; }

        public IManualRegistration Registration => DependencyContainerOptions
            .DelegateRegistration(container =>
            {
                container.RegisterInstance<IIntegrationTransport>(this);
                container.RegisterInstance<InMemoryIntegrationTransport>(this);

                container.Register<IUbiquitousIntegrationContext, InMemoryIntegrationContext>(EnLifestyle.Scoped);
                container.Register<IExtendedIntegrationContext, InMemoryIntegrationContext>(EnLifestyle.Scoped);
                container.Register<InMemoryIntegrationContext, InMemoryIntegrationContext>(EnLifestyle.Scoped);

                container.RegisterDecorator<IExtendedIntegrationContext, ExtendedIntegrationContextHeadersMaintenanceDecorator>(EnLifestyle.Scoped);
                container.RegisterDecorator<IExtendedIntegrationContext, ExtendedIntegrationContextRequirementsDecorator>(EnLifestyle.Scoped);

                container.RegisterInstance<IEndpointInstanceSelectionBehavior>(_selectionBehavior);
                container.RegisterInstance(_selectionBehavior.GetType(), _selectionBehavior);
            });

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return _rootScope.DisposeAsync();
        }

        /// <inheritdoc />
        public Task Initialize(IEnumerable<IGenericEndpoint> endpoints, CancellationToken token)
        {
            foreach (var endpoint in endpoints)
            {
                foreach (var command in endpoint.IntegrationTypeProvider.EndpointCommands())
                {
                    AddMessageTarget(command, endpoint);
                }

                foreach (var query in endpoint.IntegrationTypeProvider.EndpointQueries())
                {
                    AddMessageTarget(query, endpoint);
                }

                foreach (var integrationEvent in endpoint.IntegrationTypeProvider.EndpointSubscriptions())
                {
                    AddMessageTarget(integrationEvent, endpoint);
                }
            }

            _manualResetEvent.Set();

            return Task.CompletedTask;

            static void AddMessageTarget(Type message, IGenericEndpoint endpoint)
            {
                var logicalNameMap = TopologyMap
                    .GetOrAdd(message, _ => new Dictionary<string, IReadOnlyCollection<IGenericEndpoint>>(StringComparer.OrdinalIgnoreCase));

                lock (logicalNameMap)
                {
                    if (logicalNameMap.TryGetValue(endpoint.Identity.LogicalName, out var collection))
                    {
                        ((List<IGenericEndpoint>)collection).Add(endpoint);
                    }
                    else
                    {
                        logicalNameMap[endpoint.Identity.LogicalName] = new List<IGenericEndpoint> { endpoint };
                    }
                }
            }
        }

        /// <inheritdoc />
        public Task DispatchToEndpoint(IntegrationMessage message)
        {
            if (TopologyMap.TryGetValue(message.ReflectedType, out var endpoints))
            {
                var selectedEndpoints = endpoints
                    .Select(grp => SelectEndpointInstance(message, grp.Value))
                    .ToList();

                if (selectedEndpoints.Any())
                {
                    var runningHandlers = selectedEndpoints.Select(endpoint => DispatchToEndpointInstance(message, endpoint));

                    return Task.WhenAll(runningHandlers);
                }
            }

            throw new NotFoundException($"Target endpoint for message '{message.ReflectedType.FullName}' not found");
        }

        internal async Task NotifyOnMessage(IntegrationMessage message, CancellationToken token)
        {
            await _manualResetEvent.WaitAsync(token).ConfigureAwait(false);

            OnMessage?.Invoke(this, new IntegrationMessageEventArgs(message));
        }

        private IGenericEndpoint SelectEndpointInstance(IntegrationMessage message, IReadOnlyCollection<IGenericEndpoint> endpoints)
        {
            return _selectionBehavior.SelectInstance(message, endpoints);
        }

        private async Task DispatchToEndpointInstance(IntegrationMessage message, IGenericEndpoint endpoint)
        {
            var messageCopy = message.DeepCopy();
            var runtimeInfo = new EndpointRuntimeInfo(endpoint.Identity, messageCopy);

            await using (DependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = DependencyContainer.Resolve<IExtendedIntegrationContext, EndpointRuntimeInfo>(runtimeInfo);

                await ((IExecutableEndpoint)endpoint).InvokeMessageHandler(exclusiveContext).ConfigureAwait(false);
            }
        }
    }
}