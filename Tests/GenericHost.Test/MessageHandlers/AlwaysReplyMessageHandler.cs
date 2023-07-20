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