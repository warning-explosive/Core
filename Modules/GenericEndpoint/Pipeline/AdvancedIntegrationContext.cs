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
    using Contract.Extensions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Messaging.Abstractions;
    using Messaging.MessageHeaders;
    using RpcRequest;

    [Component(EnLifestyle.Scoped)]
    internal class AdvancedIntegrationContext : IAdvancedIntegrationContext,
                                                IResolvable<IIntegrationContext>,
                                                IResolvable<IAdvancedIntegrationContext>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationMessageFactory _factory;
        private readonly IIntegrationTransport _transport;
        private readonly IRpcRequestRegistry _rpcRequestRegistry;
        private readonly IMessagesCollector _messagesCollector;

        private IntegrationMessage? _message;

        public AdvancedIntegrationContext(
            EndpointIdentity endpointIdentity,
            IIntegrationMessageFactory factory,
            IIntegrationTransport transport,
            IRpcRequestRegistry rpcRequestRegistry,
            IMessagesCollector messagesCollector)
        {
            _endpointIdentity = endpointIdentity;
            _factory = factory;
            _transport = transport;
            _rpcRequestRegistry = rpcRequestRegistry;
            _messagesCollector = messagesCollector;
        }

        public IntegrationMessage Message => _message.EnsureNotNull($"{nameof(IAdvancedIntegrationContext)} should be initialized with integration message");

        public void Initialize(IntegrationMessage message)
        {
            _message = message;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return _messagesCollector.Collect(CreateGeneralMessage(command), token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            var isOwnedByCurrentEndpoint = typeof(TEvent).IsOwnedByEndpoint(_endpointIdentity);

            if (isOwnedByCurrentEndpoint)
            {
                return _messagesCollector.Collect(CreateGeneralMessage(integrationEvent), token);
            }

            throw new InvalidOperationException($"You can't publish events are owned by another endpoint. Event: {typeof(TEvent).FullName}; Required owner: {_endpointIdentity.LogicalName}");
        }

        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            return _messagesCollector.Collect(CreateGeneralMessage(query), token);
        }

        public async Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            var replyIntegrationMessage = CreateGeneralMessage(reply);

            var sentFrom = Message.ReadRequiredHeader<SentFrom>().Value;

            replyIntegrationMessage.WriteHeader(new ReplyTo(sentFrom));
            replyIntegrationMessage.WriteHeader(new HandledBy(_endpointIdentity));

            await _messagesCollector.Collect(replyIntegrationMessage, token).ConfigureAwait(false);
        }

        public async Task<IntegrationMessage?> TryEnrollRpcRequest<TQuery, TReply>(
            TQuery query,
            TaskCompletionSource<IntegrationMessage> tcs,
            CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            var message = CreateGeneralMessage(query);

            var requestId = message.ReadRequiredHeader<Id>().Value;

            var wasEnrolled = await _rpcRequestRegistry
               .TryEnroll(requestId, tcs, token)
               .ConfigureAwait(false);

            return wasEnrolled
                ? message
                : default;
        }

        public Task<bool> SendMessage(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
        }

        public Task Accept(CancellationToken token)
        {
            return _transport.Accept(Message, token);
        }

        public Task Reject(Exception exception, CancellationToken token)
        {
            return _transport.EnqueueError(_endpointIdentity, Message, exception, token);
        }

        public async Task Retry(TimeSpan dueTime, CancellationToken token)
        {
            var copy = Message.Clone();

            copy.OverwriteHeader(new RetryCounter((copy.ReadHeader<RetryCounter>()?.Value ?? 0) + 1));
            copy.OverwriteHeader(new DeferredUntil(DateTime.UtcNow + dueTime));

            var wasSent = await SendMessage(copy, token).ConfigureAwait(false);

            if (!wasSent)
            {
                throw new InvalidOperationException("Retry wasn't successful");
            }

            await _transport
               .Accept(Message, token)
               .ConfigureAwait(false);
        }

        private IntegrationMessage CreateGeneralMessage<TMessage>(
            TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, _endpointIdentity, _message);
        }
    }
}