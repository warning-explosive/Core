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
                                                 ICollectionResolvable<IMessageHandler<Query>>
    {
        public Task Handle(Query message, IIntegrationContext context, CancellationToken token)
        {
            return message.Id % 2 == 0
                ? Task.CompletedTask
                : context.Reply(message, new Reply(message.Id), token);
        }
    }
}