namespace SpaceEngineers.Core.GenericEndpoint.Implementations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Contract;
    using Contract.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Messaging.Abstractions;
    using Messaging.MessageHeaders;

    [Component(EnLifestyle.Scoped)]
    internal class AdvancedIntegrationContext : IAdvancedIntegrationContext
    {
        private readonly IIntegrationTransport _transport;
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
            _endpointIdentity = endpointIdentity;
            _factory = factory;

            UnitOfWork = unitOfWork;
        }

        public IntegrationMessage Message => _message.EnsureNotNull($"{nameof(IAdvancedIntegrationContext)} should be initialized with integration message");

        public IIntegrationUnitOfWork UnitOfWork { get; }

        public void Initialize(IntegrationMessage message)
        {
            _message = message;
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
            where TReply : IIntegrationReply
        {
            return Gather(CreateGeneralMessage(query), token);
        }

        public async Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            // TODO: #139 - fix reply behavior
            var sentFrom = Message.ReadRequiredHeader<SentFrom>().Value;

            var replyIntegrationMessage = CreateGeneralMessage(reply);

            await Gather(replyIntegrationMessage, token).ConfigureAwait(false);

            if (Message.ReadHeader<DidHandlerReplyToTheQuery>()?.Value == true)
            {
                throw new InvalidOperationException("Message handler already replied to integration query");
            }

            Message.WriteHeader(new DidHandlerReplyToTheQuery(true));
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            var copy = Message.Clone();

            copy.IncrementRetryCounter();
            copy.DeferDelivery(dueTime);

            return _transport.Enqueue(copy, token);
        }

        public Task Refuse(Exception exception, CancellationToken token)
        {
            return _transport.EnqueueError(Message, exception, token);
        }

        private IntegrationMessage CreateGeneralMessage<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, _endpointIdentity, Message);
        }

        private Task Gather(IntegrationMessage message, CancellationToken token)
        {
            return UnitOfWork.OutboxStorage.Add(message, token);
        }
    }
}