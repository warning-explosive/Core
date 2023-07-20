namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class EventEmptyMessageHandler : IMessageHandler<Event>,
                                              IResolvable<IMessageHandler<Event>>
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