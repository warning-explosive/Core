namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using Messages;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class EventEmptyMessageHandler : IMessageHandler<Event>,
                                              ICollectionResolvable<IMessageHandler<Event>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public EventEmptyMessageHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(Event message, CancellationToken token)
        {
            if (_endpointIdentity.LogicalName.Equals(TestIdentity.Endpoint1, StringComparison.OrdinalIgnoreCase))
            {
                return _context.Publish(new Endpoint1HandlerInvoked(typeof(EventEmptyMessageHandler), _endpointIdentity), token);
            }

            if (_endpointIdentity.LogicalName.Equals(TestIdentity.Endpoint2, StringComparison.OrdinalIgnoreCase))
            {
                return _context.Publish(new Endpoint2HandlerInvoked(typeof(EventEmptyMessageHandler), _endpointIdentity), token);
            }

            throw new NotSupportedException(_endpointIdentity.ToString());
        }
    }
}