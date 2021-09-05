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
        public Task Handle(Query message, IIntegrationContext context, CancellationToken token)
        {
            return context.Reply(message, new Reply(message.Id), token);
        }
    }
}