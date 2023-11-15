namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class MakeRequestCommandHandler : IMessageHandler<MakeRequestCommand>,
                                               IResolvable<IMessageHandler<MakeRequestCommand>>
    {
        private readonly IIntegrationContext _context;

        public MakeRequestCommandHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(MakeRequestCommand message, CancellationToken token)
        {
            var request = new Request(message.Id);

            return _context.Request<Request, Reply>(request, token);
        }
    }
}