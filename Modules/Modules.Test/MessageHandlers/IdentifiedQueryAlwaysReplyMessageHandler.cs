namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class IdentifiedQueryAlwaysReplyMessageHandler : IMessageHandler<IdentifiedQuery>
    {
        public Task Handle(IdentifiedQuery message, IIntegrationContext context, CancellationToken token)
        {
            return context.Reply(message, new IdentifiedReply(message.Id), token);
        }
    }
}