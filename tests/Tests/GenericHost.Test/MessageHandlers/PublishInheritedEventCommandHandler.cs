namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
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
                                                         IResolvable<IMessageHandler<PublishInheritedEventCommand>>
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