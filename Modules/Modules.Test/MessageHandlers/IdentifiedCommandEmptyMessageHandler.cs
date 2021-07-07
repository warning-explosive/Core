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
    internal class IdentifiedCommandEmptyMessageHandler : MessageHandlerBase<IdentifiedCommand>
    {
        public override Task Handle(IdentifiedCommand message, IIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}