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

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            var message = CreateGeneralMessage(command, typeof(TCommand));

            return _messagesCollector.Collect(message, token);
        }

        public Task Delay<TCommand>(TCommand command, TimeSpan dueTime, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Delay(command, DateTime.UtcNow + dueTime, token);
        }

        public Task Delay<TCommand>(TCommand command, DateTime dateTime, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            var message = CreateGeneralMessage(command, typeof(TCommand), new DeferredUntil(dateTime.ToUniversalTime()));

            return _messagesCollector.Collect(message, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            var isOwnedByCurrentEndpoint = typeof(TEvent).IsOwnedByEndpoint(_endpointIdentity);

            if (isOwnedByCurrentEndpoint)
            {
                return _messagesCollector.Collect(CreateGeneralMessage(integrationEvent, typeof(TEvent)), token);
            }

            throw new InvalidOperationException($"You can't publish events are owned by another endpoint. Event: {typeof(TEvent).FullName}; Required owner: {_endpointIdentity.LogicalName}; Actual owner: {typeof(TEvent).GetAttribute<OwnedByAttribute>().EndpointName};");
        }

        public Task Request<TRequest, TReply>(TRequest request, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            return _messagesCollector.Collect(CreateGeneralMessage(request, typeof(TRequest)), token);
        }

        public Task<TReply> RpcRequest<TRequest, TReply>(TRequest request, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            var message = CreateGeneralMessage(request, typeof(TRequest));

            throw new NotImplementedException("#205");
        }

        public Task Reply<TRequest, TReply>(TRequest request, TReply reply, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            var message = CreateGeneralMessage(reply, typeof(TReply));

            return _messagesCollector.Collect(message, token);
        }

        public Task<bool> SendMessage(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
        }

        public Task Reject(Exception exception, CancellationToken token)
        {
            Message.WriteHeader(new RejectReason(exception));

            return Task.CompletedTask;
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