namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;

    internal class InMemoryIntegrationContext : IExtendedIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;

        public InMemoryIntegrationContext(InMemoryIntegrationTransport transport)
        {
            _transport = transport;
        }

        internal EndpointIdentity? EndpointIdentity { get; private set; }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Notify(command, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Notify(integrationEvent, token);
        }

        public Task Request<TQuery, TResponse>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            return Notify(query, token);
        }

        public Task Reply<TQuery, TResponse>(TQuery query, TResponse response, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            return Notify(response, token);
        }

        public Task Retry<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            return Notify(message, token);
        }

        internal InMemoryIntegrationContext WithinEndpointScope(EndpointIdentity endpointIdentity)
        {
            EndpointIdentity = endpointIdentity;
            return this;
        }

        private Task Notify<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            return _transport.NotifyOnMessage(message, token);
        }
    }
}