namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Attributes;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using Internals;

    [ManualRegistration]
    internal class InMemoryIntegrationContext : IExtendedIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;
        private readonly ICollection<IntegrationMessage> _messages;

        private EndpointScope? _endpointScope;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            IIntegrationMessageFactory factory)
        {
            _transport = transport;
            _factory = factory;
            _messages = new List<IntegrationMessage>();
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Gather(command, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Gather(integrationEvent, token);
        }

        public Task Request<TQuery, TResponse>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            return Gather(query, token);
        }

        public Task Reply<TQuery, TResponse>(TQuery query, TResponse response, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage
        {
            _endpointScope
                .EnsureNotNull(string.Format(Resources.EndpointScopeRequired, nameof(Reply)))
                .InitiatorMessage
                .SetReplied();

            return Gather(response, token);
        }

        public Task Retry<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            _endpointScope.EnsureNotNull(string.Format(Resources.EndpointScopeRequired, nameof(Retry)));

            var integrationMessage = CreateGeneralMessage(message).IncrementRetryCounter();

            return Deliver(integrationMessage, token);
        }

        public IAsyncDisposable WithinEndpointScope(EndpointScope endpointScope, CancellationToken token)
        {
            _endpointScope = endpointScope;
            return AsyncDisposable.Create(token, DeliverAll);
        }

        private Task Gather<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            var integrationMessage = CreateGeneralMessage(message);

            if (_endpointScope == null)
            {
                return Deliver(integrationMessage, token);
            }

            lock (_messages)
            {
                _messages.Add(integrationMessage);
            }

            return Task.CompletedTask;
        }

        private IntegrationMessage CreateGeneralMessage<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, _endpointScope?.Identity, _endpointScope?.InitiatorMessage);
        }

        private Task DeliverAll(CancellationToken token)
        {
            if (_endpointScope == null)
            {
                return Task.CompletedTask;
            }

            lock (_messages)
            {
                return Task.WhenAll(_messages.Select(message => Deliver(message, token)));
            }
        }

        private Task Deliver(IntegrationMessage message, CancellationToken token)
        {
            return _transport.NotifyOnMessage(message, token);
        }
    }
}