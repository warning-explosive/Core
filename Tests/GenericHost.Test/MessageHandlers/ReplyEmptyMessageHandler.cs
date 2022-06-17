namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using Messages;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class ReplyEmptyMessageHandler : IMessageHandler<Reply>,
                                              IResolvable<IMessageHandler<Reply>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public ReplyEmptyMessageHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(Reply message, CancellationToken token)
        {
            return _context.Publish(new Endpoint1HandlerInvoked(typeof(ReplyEmptyMessageHandler), _endpointIdentity), token);
        }
    }
}