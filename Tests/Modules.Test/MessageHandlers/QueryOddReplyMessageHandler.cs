namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class QueryOddReplyMessageHandler : IMessageHandler<Query>,
                                                 IResolvable<IMessageHandler<Query>>
    {
        private readonly IIntegrationContext _context;

        public QueryOddReplyMessageHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(Query message, CancellationToken token)
        {
            return message.Id % 2 == 0
                ? Task.CompletedTask
                : _context.Reply(message, new Reply(message.Id), token);
        }
    }
}