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

        private EndpointIdentity? _endpointIdentity;
        private IntegrationMessage? _integrationMessage;

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
            var integrationMessage = _factory.CreateGeneralMessage(command, _endpointIdentity, _integrationMessage);
            return Gather(integrationMessage, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            var integrationMessage = _factory.CreateGeneralMessage(integrationEvent, _endpointIdentity, _integrationMessage);
            return Gather(integrationMessage, token);
        }

        public Task Request<TQuery, TResponse>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            var integrationMessage = _factory.CreateGeneralMessage(query, _endpointIdentity, _integrationMessage);
            return Gather(integrationMessage, token);
        }

        public async Task Reply<TQuery, TResponse>(TQuery query, TResponse response, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            var initiatorIntegrationMessage = _integrationMessage
                .EnsureNotNull(string.Format(Resources.EndpointScopeRequired, nameof(Reply)))
                .SetReplied();

            var integrationMessage = _factory.CreateGeneralMessage(response, _endpointIdentity, initiatorIntegrationMessage);

            await Gather(integrationMessage, token).ConfigureAwait(false);
        }

        public async Task Retry<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            var initiatorIntegrationMessage = _integrationMessage
                .EnsureNotNull(string.Format(Resources.EndpointScopeRequired, nameof(Retry)));

            var integrationMessage = _factory
                .CreateGeneralMessage(message, _endpointIdentity, initiatorIntegrationMessage)
                .IncrementRetryCounter();

            await Gather(integrationMessage, token).ConfigureAwait(false);
        }

        internal InMemoryIntegrationContext WithinEndpointScope(
            EndpointIdentity endpointIdentity,
            IntegrationMessage integrationMessage)
        {
            _endpointIdentity = endpointIdentity;
            _integrationMessage = integrationMessage;

            return this;
        }

        private Task Gather(IntegrationMessage message, CancellationToken token)
        {
            // TODO: Gather messages until message handler is running and send all as batch
            return _transport.NotifyOnMessage(message, token);
        }
    }
}