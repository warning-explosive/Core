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
    internal class ReplyEmptyMessageHandler : IMessageHandler<Reply>,
                                              ICollectionResolvable<IMessageHandler<Reply>>
    {
        public Task Handle(Reply message, IIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}