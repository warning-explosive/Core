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
    internal class ReplyEmptyMessageHandler : IMessageHandler<Reply>,
                                              ICollectionResolvable<IMessageHandler<Reply>>
    {
        private readonly EndpointIdentity _endpointIdentity;

        public ReplyEmptyMessageHandler(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public Task Handle(Reply message, IIntegrationContext context, CancellationToken token)
        {
            return context.Publish(new Endpoint1HandlerInvoked(typeof(ReplyEmptyMessageHandler), _endpointIdentity), token);
        }
    }
}