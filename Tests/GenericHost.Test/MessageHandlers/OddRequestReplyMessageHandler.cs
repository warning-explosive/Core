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
    internal class OddRequestReplyMessageHandler : IMessageHandler<Request>,
                                                   IResolvable<IMessageHandler<Request>>
    {
        private readonly IIntegrationContext _context;

        public OddRequestReplyMessageHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(Request message, CancellationToken token)
        {
            return message.Id % 2 == 0
                ? Task.CompletedTask
                : _context.Reply(message, new Reply(message.Id), token);
        }
    }
}