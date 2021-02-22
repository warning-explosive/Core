namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using Internals;

    internal class InMemoryIntegrationContext : IExtendedIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;

        private EndpointScope? _endpointScope;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            IIntegrationMessageFactory factory)
        {
            _transport = transport;
            _factory = factory;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            var integrationMessage = _factory.CreateGeneralMessage(command, _endpointScope);
            return Gather(integrationMessage, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            var integrationMessage = _factory.CreateGeneralMessage(integrationEvent, _endpointScope);
            return Gather(integrationMessage, token);
        }

        public Task Request<TQuery, TResponse>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            var integrationMessage = _factory.CreateGeneralMessage(query, _endpointScope);
            return Gather(integrationMessage, token);
        }

        public async Task Reply<TQuery, TResponse>(TQuery query, TResponse response, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            _endpointScope
                .EnsureNotNull(string.Format(Resources.EndpointScopeRequired, nameof(Reply)))
                .InitiatorMessage
                .SetReplied();

            var integrationMessage = _factory.CreateGeneralMessage(response, _endpointScope);

            await Gather(integrationMessage, token).ConfigureAwait(false);
        }

        public async Task Retry<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            _endpointScope.EnsureNotNull(string.Format(Resources.EndpointScopeRequired, nameof(Retry)));

            var integrationMessage = _factory
                .CreateGeneralMessage(message, _endpointScope)
                .IncrementRetryCounter();

            await Gather(integrationMessage, token).ConfigureAwait(false);
        }

        internal InMemoryIntegrationContext WithinEndpointScope(EndpointScope endpointScope)
        {
            _endpointScope = endpointScope;
            return this;
        }

        private Task Gather(IntegrationMessage message, CancellationToken token)
        {
            // TODO: Gather messages until message handler is running and send all as batch
            return _transport.NotifyOnMessage(message, token);
        }
    }
}