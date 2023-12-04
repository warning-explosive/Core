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
    internal class MakeRpcRequestCommandHandler : IMessageHandler<MakeRpcRequestCommand>,
                                                  IResolvable<IMessageHandler<MakeRpcRequestCommand>>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationContext _context;

        public MakeRpcRequestCommandHandler(EndpointIdentity endpointIdentity, IIntegrationContext context)
        {
            _endpointIdentity = endpointIdentity;
            _context = context;
        }

        public Task Handle(MakeRpcRequestCommand message, CancellationToken token)
        {
            // TODO: #205 - recode back
            /*var reply = await _context
                .RpcRequest<Request, Reply>(new Request(message.Id), token)
                .ConfigureAwait(false);*/

            _context.Publish(new HandlerInvoked(typeof(MakeRpcRequestCommandHandler), _endpointIdentity));

            return Task.CompletedTask;
        }
    }
}