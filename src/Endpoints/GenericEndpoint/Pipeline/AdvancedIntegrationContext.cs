namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Messaging.MessageHeaders;

    [Component(EnLifestyle.Scoped)]
    internal class AdvancedIntegrationContext : IAdvancedIntegrationContext,
                                                IResolvable<IIntegrationContext>,
                                                IResolvable<IAdvancedIntegrationContext>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationMessageFactory _factory;
        private readonly IIntegrationTransport _transport;
        private readonly IMessagesCollector _messagesCollector;

        private IntegrationMessage? _message;

        public AdvancedIntegrationContext(
            EndpointIdentity endpointIdentity,
            IIntegrationMessageFactory factory,
            IIntegrationTransport transport,
            IMessagesCollector messagesCollector)
        {
            _endpointIdentity = endpointIdentity;
            _factory = factory;
            _transport = transport;
            _messagesCollector = messagesCollector;
        }

        public IntegrationMessage Message => _message ?? throw new InvalidOperationException($"{nameof(IAdvancedIntegrationContext)} should be initialized with integration message");

        public void Initialize(IntegrationMessage message)
        {
            _message = message;
        }

        public void Send<TCommand>(TCommand command)
            where TCommand : IIntegrationCommand
        {
            var message = CreateGeneralMessage(command, typeof(TCommand));

            _messagesCollector.Collect(message);
        }

        public void Delay<TCommand>(TCommand command, TimeSpan dueTime)
            where TCommand : IIntegrationCommand
        {
            Delay(command, DateTime.UtcNow + dueTime);
        }

        public void Delay<TCommand>(TCommand command, DateTime dateTime)
            where TCommand : IIntegrationCommand
        {
            var message = CreateGeneralMessage(command, typeof(TCommand), new DeferredUntil(dateTime.ToUniversalTime()));

            _messagesCollector.Collect(message);
        }

        public void Publish<TEvent>(TEvent integrationEvent)
            where TEvent : IIntegrationEvent
        {
            var isOwnedByCurrentEndpoint = typeof(TEvent).IsOwnedByEndpoint(_endpointIdentity);

            if (!isOwnedByCurrentEndpoint)
            {
                throw new InvalidOperationException($"You can't publish events are owned by another endpoint. Event: {typeof(TEvent).FullName}; Required owner: {_endpointIdentity.LogicalName}; Actual owner: {typeof(TEvent).GetAttribute<OwnedByAttribute>().EndpointName};");
            }

            var message = CreateGeneralMessage(integrationEvent, typeof(TEvent));

            _messagesCollector.Collect(message);
        }

        public void Request<TRequest, TReply>(TRequest request)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            var message = CreateGeneralMessage(request, typeof(TRequest));

            _messagesCollector.Collect(message);
        }

        public void Reply<TRequest, TReply>(TRequest request, TReply reply)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            var message = CreateGeneralMessage(reply, typeof(TReply));

            _messagesCollector.Collect(message);
        }

        public Task<bool> SendMessage(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
        }

        public void Reject(Exception exception)
        {
            Message.WriteHeader(new RejectReason(exception));
        }

        public Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            return Retry(DateTime.UtcNow + dueTime, token);
        }

        public async Task Retry(DateTime dateTime, CancellationToken token)
        {
            var copy = CreateGeneralMessage(
                Message.Payload,
                Message.ReflectedType,
                new RetryCounter((Message.ReadHeader<RetryCounter>()?.Value ?? 0) + 1),
                new DeferredUntil(dateTime.ToUniversalTime()));

            var wasSent = await SendMessage(copy, token).ConfigureAwait(false);

            if (!wasSent)
            {
                throw new InvalidOperationException("Retry wasn't successful");
            }
        }

        private IntegrationMessage CreateGeneralMessage<TMessage>(
            TMessage message,
            Type reflectedType,
            params IIntegrationMessageHeader[] headers)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, reflectedType, headers, _message);
        }
    }
}