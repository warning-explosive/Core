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
    internal class PublishInheritedEventCommandHandler : IMessageHandler<PublishInheritedEventCommand>,
                                                         ICollectionResolvable<IMessageHandler<PublishInheritedEventCommand>>
    {
        private readonly IIntegrationContext _context;

        public PublishInheritedEventCommandHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(PublishInheritedEventCommand message, CancellationToken token)
        {
            return _context.Publish(new InheritedEvent(message.Id), token);
        }
    }
}