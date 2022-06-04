namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using Messages;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class IntegrationEventEmptyMessageHandler : IMessageHandler<IIntegrationEvent>,
                                                         ICollectionResolvable<IMessageHandler<IIntegrationEvent>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public IntegrationEventEmptyMessageHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(IIntegrationEvent message, CancellationToken token)
        {
            return _context.Publish(new Endpoint1HandlerInvoked(typeof(IntegrationEventEmptyMessageHandler), _endpointIdentity), token);
        }
    }
}