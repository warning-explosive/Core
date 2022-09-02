namespace SpaceEngineers.Core.GenericEndpoint.RpcRequest
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Extensions;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
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
        private readonly IRpcRequestRegistry _rpcRequestRegistry;
        private readonly ILogger _logger;

        public RpcReplyMessageHandler(
            IIntegrationContext context,
            IRpcRequestRegistry rpcRequestRegistry,
            ILogger logger)
        {
            _context = context;
            _rpcRequestRegistry = rpcRequestRegistry;
            _logger = logger;
        }

        public Task Handle(TReply message, CancellationToken token)
        {
            var integrationMessage = ((IAdvancedIntegrationContext)_context).Message;

            var requestId = integrationMessage.ReadRequiredHeader<InitiatorMessageId>().Value;

            if (!_rpcRequestRegistry.TrySetResult(requestId, integrationMessage))
            {
                _logger.Warning($"Rpc request {requestId} was timed out");
            }

            return Task.CompletedTask;
        }
    }
}