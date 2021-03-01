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

    [ManualRegistration]
    internal class InMemoryIntegrationContext : IExtendedIntegrationContext
    {
        private const string EndpointContextRequired = "Method must be executed within endpoint context (in message handler)";

        private readonly InMemoryIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;
        private readonly ICollection<IntegrationMessage> _messages;

        private bool _deliverImmediately;
        private IntegrationMessage? _message;
        private EndpointIdentity? _endpointIdentity;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            IIntegrationMessageFactory factory)
        {
            _transport = transport;
            _factory = factory;
            _messages = new List<IntegrationMessage>();
            _deliverImmediately = true;
        }

        public IntegrationMessage Message => _message.EnsureNotNull(EndpointContextRequired);

        public EndpointIdentity EndpointIdentity => _endpointIdentity.EnsureNotNull(EndpointContextRequired);

        public void Initialize(EndpointRuntimeInfo info)
        {
            _message = info.Message;
            _endpointIdentity = info.EndpointIdentity;
            _deliverImmediately = false;
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

        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            return Gather(query, token);
        }

        public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            Message.SetReplied();

            return Gather(reply, token);
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            /* TODO: use due time between retries */

            Message.IncrementRetryCounter();

            return Deliver(Message, token);
        }

        public IAsyncDisposable WithinEndpointScope(AsyncUnitOfWorkBuilder<EndpointIdentity> unitOfWorkBuilder)
        {
            unitOfWorkBuilder.RegisterOnCommit(DeliverAll);
            return AsyncDisposable.Empty;
        }

        private Task Gather<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            var integrationMessage = _factory.CreateGeneralMessage(message, _endpointIdentity, _message);

            if (_deliverImmediately)
            {
                return Deliver(integrationMessage, token);
            }

            lock (_messages)
            {
                _messages.Add(integrationMessage);
            }

            return Task.CompletedTask;
        }

        private async Task DeliverAll(EndpointIdentity endpointIdentity, CancellationToken token)
        {
            ICollection<IntegrationMessage> forDelivery;

            lock (_messages)
            {
                forDelivery = _messages.ToList();
                _messages.Clear();
            }

            await Task.WhenAll(forDelivery.Select(message => Deliver(message, token))).ConfigureAwait(false);
        }

        private Task Deliver(IntegrationMessage message, CancellationToken token)
        {
            return _transport.NotifyOnMessage(message, token);
        }
    }
}