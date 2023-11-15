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
    internal class InheritedEventHandler : IMessageHandler<InheritedEvent>,
                                           IResolvable<IMessageHandler<InheritedEvent>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public InheritedEventHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(InheritedEvent message, CancellationToken token)
        {
            return _context.Publish(new HandlerInvoked(typeof(InheritedEventHandler), _endpointIdentity), token);
        }
    }
}