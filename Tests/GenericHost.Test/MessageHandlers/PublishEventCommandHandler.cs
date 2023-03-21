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
    internal class PublishEventCommandHandler : IMessageHandler<PublishEventCommand>,
                                                IResolvable<IMessageHandler<PublishEventCommand>>
    {
        private readonly IIntegrationContext _context;

        public PublishEventCommandHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(PublishEventCommand message, CancellationToken token)
        {
            return _context.Publish(new Event(message.Id), token);
        }
    }
}