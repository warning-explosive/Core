namespace SpaceEngineers.Core.InMemoryIntegrationTransport
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Basics.Exceptions;
    using GenericEndpoint.Abstractions;
    using GenericHost;
    using GenericHost.Abstractions;

    /// <summary>
    /// InMemoryIntegrationTransport
    /// </summary>
    internal class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private static readonly ConcurrentDictionary<Type, ICollection<IGenericEndpoint>> Map
            = new ConcurrentDictionary<Type, ICollection<IGenericEndpoint>>();

        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;
        private readonly IIntegrationContext _context;

        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior selectionBehavior)
        {
            _selectionBehavior = selectionBehavior;
            _context = new InMemoryIntegrationContext(this);
        }

        public event EventHandler<IntegrationMessageEventArgs>? OnMessage;

        public IIntegrationContext Context => _context;

        public Task InitializeTopology(IGenericEndpoint endpoint)
        {
            foreach (var integrationCommand in endpoint.IntegrationTypesProvider.EndpointCommands())
            {
                AddMessageTarget(integrationCommand, endpoint);
            }

            foreach (var integrationEvent in endpoint.IntegrationTypesProvider.EndpointSubscriptions())
            {
                AddMessageTarget(integrationEvent, endpoint);
            }

            return Task.CompletedTask;

            static void AddMessageTarget(Type message, IGenericEndpoint endpoint)
            {
                var collection = Map.GetOrAdd(message, _ => new List<IGenericEndpoint>());

                lock (collection)
                {
                    collection.Add(endpoint);
                }
            }
        }

        public Task Dispatch<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            if (Map.TryGetValue(typeof(TMessage), out var endpoints))
            {
                var runningHandlers = endpoints
                    .GroupBy(endpoint => endpoint.Identity.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .Select(grp => _selectionBehavior.SelectInstance(message, grp.ToList()))
                    .Select(endpoint => endpoint.InvokeMessageHandler(message, _context));

                return Task.WhenAll(runningHandlers);
            }

            throw new NotFoundException($"Target endpoint for message '{typeof(TMessage)}' not found");
        }

        internal void NotifyOnMessage<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            OnMessage.Invoke(this, new IntegrationMessageEventArgs(message, typeof(TMessage)));
        }
    }
}