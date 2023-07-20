namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
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
    internal class BaseEventEmptyMessageHandler : IMessageHandler<BaseEvent>,
                                                  IResolvable<IMessageHandler<BaseEvent>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public BaseEventEmptyMessageHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(BaseEvent message, CancellationToken token)
        {
            return _context.Publish(new Endpoint1HandlerInvoked(typeof(BaseEventEmptyMessageHandler), _endpointIdentity), token);
        }
    }
}