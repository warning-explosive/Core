namespace SpaceEngineers.Core.GenericEndpoint.RpcRequest
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Contract.Abstractions;
    using Messaging.MessageHeaders;
    using Pipeline;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class RpcReplyMessageHandler<TReply> : IMessageHandler<TReply>,
                                                    IResolvable<IMessageHandler<TReply>>
        where TReply : IIntegrationReply
    {
        private readonly IIntegrationContext _context;
        private readonly IRpcRequestRegistry _registry;

        public RpcReplyMessageHandler(
            IIntegrationContext context,
            IRpcRequestRegistry registry)
        {
            _context = context;
            _registry = registry;
        }

        public Task Handle(TReply message, CancellationToken token)
        {
            var integrationMessage = ((IAdvancedIntegrationContext)_context).Message;

            var requestId = integrationMessage.ReadRequiredHeader<InitiatorMessageId>().Value;

            _registry.TrySetResult(requestId, integrationMessage);

            return Task.CompletedTask;
        }
    }
}