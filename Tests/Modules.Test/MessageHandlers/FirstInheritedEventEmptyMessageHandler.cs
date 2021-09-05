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
    internal class FirstInheritedEventEmptyMessageHandler : IMessageHandler<FirstInheritedEvent>,
                                                            ICollectionResolvable<IMessageHandler<FirstInheritedEvent>>
    {
        public Task Handle(FirstInheritedEvent message, IIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}