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
    internal class CommandEmptyMessageHandler : IMessageHandler<Command>,
                                                IResolvable<IMessageHandler<Command>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public CommandEmptyMessageHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(Command message, CancellationToken token)
        {
            return _context.Publish(new Endpoint1HandlerInvoked(typeof(CommandEmptyMessageHandler), _endpointIdentity), token);
        }
    }
}