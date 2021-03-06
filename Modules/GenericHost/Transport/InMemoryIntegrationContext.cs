namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;

    [ManualRegistration]
    internal class InMemoryIntegrationContext : IExtendedIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;
        private readonly ICollection<IntegrationMessage> _messages;

        private bool _immediateDelivery;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            IIntegrationMessageFactory factory)
        {
            _transport = transport;
            _factory = factory;
            _messages = new List<IntegrationMessage>();
            _immediateDelivery = true;
        }

        public IntegrationMessage Message { get; private set; } = null!;

        public EndpointIdentity EndpointIdentity { get; private set; } = null!;

        public void Initialize(EndpointRuntimeInfo info)
        {
            Message = info.Message;
            EndpointIdentity = info.EndpointIdentity;
            _immediateDelivery = false;
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
            return Gather(reply, token);
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            /* TODO: use due time between retries */

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
            var integrationMessage = _factory.CreateGeneralMessage(message, EndpointIdentity, Message);

            if (_immediateDelivery)
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