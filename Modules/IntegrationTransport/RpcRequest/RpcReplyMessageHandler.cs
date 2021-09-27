namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class RpcReplyMessageHandler<TReply> : IRpcReplyMessageHandler<TReply>
        where TReply : IIntegrationReply
    {
        private readonly IRpcRequestRegistry _registry;

        public RpcReplyMessageHandler(IRpcRequestRegistry registry)
        {
            _registry = registry;
        }

        public Task Handle(IntegrationMessage message)
        {
            var requestId = message.ReadRequiredHeader<InitiatorMessageId>().Value;

            _registry.TrySetResult(requestId, message);

            return Task.CompletedTask;
        }
    }
}