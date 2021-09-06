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
    internal class PublishInheritedEventCommandHandler : IMessageHandler<PublishInheritedEventCommand>,
                                                         ICollectionResolvable<IMessageHandler<PublishInheritedEventCommand>>
    {
        public Task Handle(PublishInheritedEventCommand message, IIntegrationContext context, CancellationToken token)
        {
            return context.Publish(new InheritedEvent(message.Id), token);
        }
    }
}