namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class IntegrationEventEmptyMessageHandler : IMessageHandler<IIntegrationEvent>,
                                                         ICollectionResolvable<IMessageHandler<IIntegrationEvent>>
    {
        private readonly EndpointIdentity _endpointIdentity;

        public IntegrationEventEmptyMessageHandler(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public Task Handle(IIntegrationEvent message, IIntegrationContext context, CancellationToken token)
        {
            return context.Publish(new Endpoint1HandlerInvoked(typeof(IntegrationEventEmptyMessageHandler), _endpointIdentity), token);
        }
    }
}