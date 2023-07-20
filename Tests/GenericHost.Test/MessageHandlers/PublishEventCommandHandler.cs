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