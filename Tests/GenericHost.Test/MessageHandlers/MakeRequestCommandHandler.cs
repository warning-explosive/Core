namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messages;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;

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