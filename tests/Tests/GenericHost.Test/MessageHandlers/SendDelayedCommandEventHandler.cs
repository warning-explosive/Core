namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class SendDelayedCommandEventHandler : IMessageHandler<Event>,
                                                    IResolvable<IMessageHandler<Event>>
    {
        private readonly IIntegrationContext _context;

        public SendDelayedCommandEventHandler(IIntegrationContext context)
        {
            _context = context;
        }

        public Task Handle(Event message, CancellationToken token)
        {
            return _context.Delay(new Command(message.Id), DateTime.UtcNow + TimeSpan.FromDays(message.Id), token);
        }
    }
}