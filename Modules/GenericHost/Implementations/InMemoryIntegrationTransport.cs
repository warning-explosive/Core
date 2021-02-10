namespace SpaceEngineers.Core.GenericHost.Implementations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics.Exceptions;
    using Core.GenericEndpoint.Abstractions;
    using Core.GenericHost;
    using Internals;

    /// <summary>
    /// InMemoryIntegrationTransport
    /// </summary>
    public class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private static readonly ConcurrentDictionary<Type, ICollection<IGenericEndpoint>> TopologyMap
            = new ConcurrentDictionary<Type, ICollection<IGenericEndpoint>>();

        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;

        private readonly IIntegrationContext _context;

        // TODO: use async counterpart
        private readonly ManualResetEventSlim _manualResetEvent;

        /// <summary> .cctor </summary>
        /// <param name="selectionBehavior">IEndpointInstanceSelectionBehavior</param>
        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior selectionBehavior)
        {
            _selectionBehavior = selectionBehavior;

            _manualResetEvent = new ManualResetEventSlim(false);

            _context = new InMemoryIntegrationContext(this, _manualResetEvent);
        }

        /// <inheritdoc />
        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        /// <inheritdoc />
        public Task Initialize(IEnumerable<IGenericEndpoint> endpoints, CancellationToken cancellationToken)
        {
            foreach (var endpoint in endpoints)
            {
                foreach (var integrationCommand in endpoint.IntegrationTypesProvider.EndpointCommands())
                {
                    AddMessageTarget(integrationCommand, endpoint);
                }

                foreach (var integrationEvent in endpoint.IntegrationTypesProvider.EndpointSubscriptions())
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
        public IIntegrationContext CreateContext() => _context;

        /// <inheritdoc />
        public Task DispatchToEndpoint<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            if (TopologyMap.TryGetValue(typeof(TMessage), out var endpoints))
            {
                var selectedEndpoints = endpoints
                    .GroupBy(endpoint => endpoint.Identity.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .Select(grp => _selectionBehavior.SelectInstance(message, grp.ToList()))
                    .ToList();

                if (selectedEndpoints.Any())
                {
                    return Task.WhenAll(selectedEndpoints.Select(endpoint => ((IExecutableEndpoint)endpoint).InvokeMessageHandler(message, CreateContext())));
                }
            }

            throw new NotFoundException($"Target endpoint for message '{typeof(TMessage)}' not found");
        }

        internal Task NotifyOnMessage<TMessage>(TMessage integrationMessage)
            where TMessage : IIntegrationMessage
        {
            OnMessage?.Invoke(this, new IntegrationMessageEventArgs(integrationMessage, typeof(TMessage)));
            return Task.CompletedTask;
        }
    }
}