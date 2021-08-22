namespace SpaceEngineers.Core.IntegrationTransport.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationContext : IIntegrationContext
    {
        private const string RpcRequestMockEndpoint = nameof(RpcRequestMockEndpoint);

        private readonly IIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;
        private readonly IRpcRequestRegistry _registry;
        private readonly EndpointIdentity _rpcRequestMockEndpointIdentity;

        public IntegrationContext(
            IIntegrationTransport transport,
            IIntegrationMessageFactory factory,
            IRpcRequestRegistry registry)
        {
            _transport = transport;
            _factory = factory;
            _registry = registry;
            _rpcRequestMockEndpointIdentity = new EndpointIdentity(RpcRequestMockEndpoint, 0);
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Deliver(CreateGeneralMessage(command), token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Deliver(CreateGeneralMessage(integrationEvent), token);
        }

        public async Task<TReply> RpcRequest<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            var message = CreateGeneralMessage(query);

            message.Headers[IntegrationMessageHeader.SentFrom] = _rpcRequestMockEndpointIdentity;

            var requestId = message.Id;

            var tcs = new TaskCompletionSource<TReply>();

            await _registry.TryEnroll(requestId, tcs, token).ConfigureAwait(false);

            _transport.Bind(typeof(TReply), _rpcRequestMockEndpointIdentity, RpcReplyMessageHandler<TReply>(_registry));

            await Deliver(message, token).ConfigureAwait(false);

            return await tcs.Task.ConfigureAwait(false);
        }

        private Task Deliver(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
        }

        private IntegrationMessage CreateGeneralMessage<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, null, null);
        }

        private static Func<IntegrationMessage, Task> RpcReplyMessageHandler<TReply>(IRpcRequestRegistry registry)
            where TReply : IIntegrationMessage
        {
            return message =>
            {
                var requestId = message.ReadRequiredHeader<Guid>(IntegrationMessageHeader.RequestId);

                // TODO: think about lost headers
                registry.TrySetResult(requestId, (TReply)message.Payload);

                return Task.CompletedTask;
            };
        }
    }
}