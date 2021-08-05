namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Contract;
    using Contract.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Messaging.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class AdvancedIntegrationContext : IAdvancedIntegrationContext
    {
        private readonly IIntegrationTransport _transport;
        private readonly ICollection<IntegrationMessage> _outgoingMessages;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationMessageFactory _factory;

        private IntegrationMessage? _message;

        public AdvancedIntegrationContext(
            IIntegrationTransport transport,
            EndpointIdentity endpointIdentity,
            IIntegrationMessageFactory factory,
            IIntegrationUnitOfWork unitOfWork)
        {
            _transport = transport;
            _outgoingMessages = new List<IntegrationMessage>();
            _endpointIdentity = endpointIdentity;
            _factory = factory;

            UnitOfWork = unitOfWork;
        }

        public IntegrationMessage Message => _message.EnsureNotNull($"{nameof(IAdvancedIntegrationContext)} should be initialized with integration message");

        public IIntegrationUnitOfWork UnitOfWork { get; }

        public void Initialize(IntegrationMessage inputData)
        {
            _message = inputData;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Gather(CreateGeneralMessage(command), token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Gather(CreateGeneralMessage(integrationEvent), token);
        }

        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            return Gather(CreateGeneralMessage(query), token);
        }

        public async Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            // TODO: fix reply behavior
            var sentFrom = Message.ReadRequiredHeader<EndpointIdentity>(IntegrationMessageHeader.SentFrom);

            await Gather(CreateGeneralMessage(reply), token).ConfigureAwait(false);

            Message.MarkAsReplied();
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            var copy = Message.Clone();

            copy.IncrementRetryCounter();
            copy.DeferDelivery(dueTime);

            return Deliver(copy, token);
        }

        public Task Refuse(Exception exception, CancellationToken token)
        {
            return _transport.EnqueueError(Message, exception, token);
        }

        public Task Deliver(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
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

        private IntegrationMessage CreateGeneralMessage<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, _endpointIdentity, Message);
        }

        private Task Gather(IntegrationMessage message, CancellationToken token)
        {
            // TODO: if (UnitOfWork.WasFinished)
            if (true)
            {
                return Deliver(message, token);
            }

            /*lock (_outgoingMessages)
            {
                _outgoingMessages.Add(message);
            }

            return Task.CompletedTask;*/
        }
    }
}