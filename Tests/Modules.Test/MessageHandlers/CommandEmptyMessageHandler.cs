namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class CommandEmptyMessageHandler : IMessageHandler<Command>,
                                                ICollectionResolvable<IMessageHandler<Command>>
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