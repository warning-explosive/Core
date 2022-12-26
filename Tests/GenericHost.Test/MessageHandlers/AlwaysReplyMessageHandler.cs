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
    internal class AlwaysReplyMessageHandler : IMessageHandler<Request>,
                                               IResolvable<IMessageHandler<Request>>
    {
        private readonly IIntegrationContext _context;

        public AlwaysReplyMessageHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(Request message, CancellationToken token)
        {
            return _context.Reply(message, new Reply(message.Id), token);
        }
    }
}