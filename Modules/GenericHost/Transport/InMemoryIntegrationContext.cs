namespace SpaceEngineers.Core.GenericHost.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;

    [ManualRegistration]
    [Lifestyle(EnLifestyle.Scoped)]
    internal class InMemoryIntegrationContext : IExtendedIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;

        private readonly ICollection<IntegrationMessage> _outgoingMessages;
        private IntegrationMessage? _message;

        public InMemoryIntegrationContext(
            EndpointIdentity endpointIdentity,
            InMemoryIntegrationTransport transport,
            IIntegrationMessageFactory factory,
            IIntegrationUnitOfWork unitOfWork)
        {
            EndpointIdentity = endpointIdentity;
            _transport = transport;
            _factory = factory;
            UnitOfWork = unitOfWork;

            _outgoingMessages = new List<IntegrationMessage>();
        }

        public IntegrationMessage Message => _message.EnsureNotNull($"{nameof(IIntegrationContext)} should be initialized with integration message");

        public EndpointIdentity EndpointIdentity { get; }

        public IIntegrationUnitOfWork UnitOfWork { get; }

        public void Initialize(IntegrationMessage message)
        {
            _message = message;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Gather(command);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Gather(integrationEvent);
        }

        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            return Gather(query);
        }

        public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            return Gather(reply);
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            /* TODO: use due time between retries */

            return Deliver(Message, token);
        }

        public async Task DeliverAll(CancellationToken token)
        {
            ICollection<IntegrationMessage> forDelivery;

            lock (_outgoingMessages)
            {
                forDelivery = _outgoingMessages.ToList();
                _outgoingMessages.Clear();
            }

            await forDelivery.Select(message => Deliver(message, token)).WhenAll().ConfigureAwait(false);
        }

        private Task Deliver(IntegrationMessage message, CancellationToken token)
        {
            return _transport.NotifyOnMessage(message, token);
        }

        private Task Gather<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            var integrationMessage = _factory.CreateGeneralMessage(message, EndpointIdentity, Message);

            lock (_outgoingMessages)
            {
                _outgoingMessages.Add(integrationMessage);
            }

            return Task.CompletedTask;
        }
    }
}