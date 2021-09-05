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
    internal class CommandEmptyMessageHandler : IMessageHandler<Command>,
                                                ICollectionResolvable<IMessageHandler<Command>>
    {
        public Task Handle(Command message, IIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}