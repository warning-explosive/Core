namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using Basics.Async;
    using Basics.Exceptions;
    using Core.GenericHost;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Internals;

    [ManualRegistration]
    internal class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, IReadOnlyCollection<IGenericEndpoint>>> TopologyMap
            = new ConcurrentDictionary<Type, IDictionary<string, IReadOnlyCollection<IGenericEndpoint>>>();

        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;
        private readonly IIntegrationMessageFactory _messageFactory;

        private readonly AsyncManualResetEvent _manualResetEvent;

        public InMemoryIntegrationTransport(
            IDependencyContainer dependencyContainer,
            IEndpointInstanceSelectionBehavior selectionBehavior,
            IIntegrationMessageFactory messageFactory)
        {
            DependencyContainer = dependencyContainer;

            _selectionBehavior = selectionBehavior;
            _messageFactory = messageFactory;

            _manualResetEvent = new AsyncManualResetEvent(false);
        }

        /// <inheritdoc />
        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        /// <inheritdoc />
        public IDependencyContainer DependencyContainer { get; }

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
        public IIntegrationContext CreateContext() => CreateContextInternal();

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

        private Task DispatchToEndpointInstance(IntegrationMessage message, IGenericEndpoint endpoint)
        {
            var messageCopy = message.DeepCopy();
            var endpointScope = new EndpointScope(endpoint.Identity, messageCopy);
            var exclusiveContext = CreateContextInternal().WithinEndpointScope(endpointScope);

            return ((IExecutableEndpoint)endpoint).InvokeMessageHandler(messageCopy, exclusiveContext);
        }

        private InMemoryIntegrationContext CreateContextInternal()
            => new InMemoryIntegrationContext(this, _messageFactory);
    }
}