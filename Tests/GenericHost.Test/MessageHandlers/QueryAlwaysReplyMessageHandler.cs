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