namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics;
    using Basics.Async;
    using Basics.Exceptions;
    using Core.GenericHost;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using InternalAbstractions;

    /// <summary>
    /// InMemoryIntegrationTransport
    /// </summary>
    public class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private static readonly ConcurrentDictionary<Type, ICollection<IGenericEndpoint>> TopologyMap
            = new ConcurrentDictionary<Type, ICollection<IGenericEndpoint>>();

        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;
        private readonly AsyncManualResetEvent _manualResetEvent;

        /// <summary> .cctor </summary>
        /// <param name="selectionBehavior">IEndpointInstanceSelectionBehavior</param>
        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior selectionBehavior)
        {
            _selectionBehavior = selectionBehavior;
            _manualResetEvent = new AsyncManualResetEvent(false);
        }

        /// <inheritdoc />
        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

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
                var collection = TopologyMap.GetOrAdd(message, _ => new List<IGenericEndpoint>());

                lock (collection)
                {
                    collection.Add(endpoint);
                }
            }
        }

        /// <inheritdoc />
        public IIntegrationContext CreateContext() => CreateContext(null);

        /// <inheritdoc />
        public Task DispatchToEndpoint<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            if (TopologyMap.TryGetValue(typeof(TMessage), out var endpoints))
            {
                var selectedEndpoints = endpoints
                    .GroupBy(endpoint => endpoint.Identity.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .Select(grp => SelectEndpointInstance(message, grp.ToList()))
                    .ToList();

                if (selectedEndpoints.Any())
                {
                    var runningHandlers = selectedEndpoints.Select(endpoint => DispatchToEndpointInstance(message, endpoint));

                    return Task.WhenAll(runningHandlers);
                }
            }

            throw new NotFoundException($"Target endpoint for message '{typeof(TMessage)}' not found");
        }

        internal async Task NotifyOnMessage<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            await _manualResetEvent.WaitAsync(token).ConfigureAwait(false);

            OnMessage?.Invoke(this, new IntegrationMessageEventArgs(message, typeof(TMessage)));
        }

        private IGenericEndpoint SelectEndpointInstance<TMessage>(TMessage message, IReadOnlyCollection<IGenericEndpoint> endpoints)
            where TMessage : IIntegrationMessage
        {
            return _selectionBehavior.SelectInstance(message, endpoints);
        }

        private Task DispatchToEndpointInstance<TMessage>(TMessage message, IGenericEndpoint endpoint)
            where TMessage : IIntegrationMessage
        {
            var messageCopy = message.DeepCopy();
            var exclusiveContext = CreateContext(endpoint.Identity);

            return ((IExecutableEndpoint)endpoint).InvokeMessageHandler(messageCopy, exclusiveContext);
        }

        private InMemoryIntegrationContext CreateContext(EndpointIdentity? endpointIdentity)
        {
            var context = new InMemoryIntegrationContext(this);

            return endpointIdentity != null
                ? context.WithinEndpointScope(endpointIdentity)
                : context;
        }
    }
}