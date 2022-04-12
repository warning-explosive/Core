namespace SpaceEngineers.Core.IntegrationTransport.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationContext : IIntegrationContext,
                                        IResolvable<IIntegrationContext>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;
        private readonly IRpcRequestRegistry _registry;

        public IntegrationContext(
            EndpointIdentity endpointIdentity,
            IIntegrationTransport transport,
            IIntegrationMessageFactory factory,
            IRpcRequestRegistry registry)
        {
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _factory = factory;
            _registry = registry;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Deliver(CreateGeneralMessage(command), token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            var actualOwner = typeof(TEvent)
                .GetRequiredAttribute<OwnedByAttribute>()
                .EndpointName;

            var isOwnedByCurrentEndpoint = actualOwner.Equals(_endpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase);

            if (isOwnedByCurrentEndpoint)
            {
                return Deliver(CreateGeneralMessage(integrationEvent), token);
            }

            throw new InvalidOperationException($"You can't publish events are owned by another endpoint. Event: {typeof(TEvent).FullName}; Owner: {actualOwner}; Required owner: {_endpointIdentity.LogicalName}");
        }

        public async Task<TReply> RpcRequest<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            var message = CreateGeneralMessage(query);

            message.WriteHeader(new SentFrom(_endpointIdentity));

            var requestId = message.ReadRequiredHeader<Id>().Value;

            var tcs = new TaskCompletionSource<IntegrationMessage>();

            await _registry.TryEnroll(requestId, tcs, token).ConfigureAwait(false);

            await Deliver(message, token).ConfigureAwait(false);

            var reply = await tcs.Task.ConfigureAwait(false);

            return (TReply)reply.Payload;
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
    }
}