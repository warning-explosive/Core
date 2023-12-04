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
    internal class OddReplyRequestHandler : IMessageHandler<Request>,
                                            IResolvable<IMessageHandler<Request>>
    {
        private readonly IIntegrationContext _context;

        public OddReplyRequestHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(Request message, CancellationToken token)
        {
            if (message.Id % 2 != 0)
            {
                _context.Reply(message, new Reply(message.Id));
            }

            return Task.CompletedTask;
        }
    }
}