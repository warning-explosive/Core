namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Api;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class IdentifiedEventEmptyMessageHandler : MessageHandlerBase<IdentifiedEvent>
    {
        public override Task Handle(IdentifiedEvent message, IIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}