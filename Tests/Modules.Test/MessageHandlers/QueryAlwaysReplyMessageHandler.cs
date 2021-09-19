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
    internal class QueryAlwaysReplyMessageHandler : IMessageHandler<Query>,
                                                    ICollectionResolvable<IMessageHandler<Query>>
    {
        private readonly IIntegrationContext _context;

        public QueryAlwaysReplyMessageHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(Query message, CancellationToken token)
        {
            return _context.Reply(message, new Reply(message.Id), token);
        }
    }
}