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
    internal class EventEmptyMessageHandler : IMessageHandler<Event>,
                                              ICollectionResolvable<IMessageHandler<Event>>
    {
        public Task Handle(Event message, IIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}