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
    internal class BaseEventEmptyMessageHandler : IMessageHandler<BaseEvent>,
                                                  ICollectionResolvable<IMessageHandler<BaseEvent>>
    {
        public async Task Handle(BaseEvent message, IIntegrationContext context, CancellationToken token)
        {
            await context.Publish(new FirstInheritedEvent(), token).ConfigureAwait(false);
            await context.Publish(new SecondInheritedEvent(), token).ConfigureAwait(false);
        }
    }
}