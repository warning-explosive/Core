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
    internal class OpenGenericCommandEmptyMessageHandler<TCommand> : IMessageHandler<TCommand>,
                                                                     IResolvable<IMessageHandler<TCommand>>
        where TCommand : OpenGenericHandlerCommand
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public OpenGenericCommandEmptyMessageHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(TCommand message, CancellationToken token)
        {
            return _context.Publish(new Endpoint1HandlerInvoked(typeof(OpenGenericCommandEmptyMessageHandler<TCommand>), _endpointIdentity), token);
        }
    }
}