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
    internal class PublishEventCommandHandler : IMessageHandler<PublishEventCommand>,
                                                ICollectionResolvable<IMessageHandler<PublishEventCommand>>
    {
        public Task Handle(PublishEventCommand message, IIntegrationContext context, CancellationToken token)
        {
            return context.Publish(new Event(message.Id), token);
        }
    }
}