namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class ThrowingMessageHandler : IMessageHandler<IdentifiedCommand>
    {
        public Task Handle(IdentifiedCommand message, IIntegrationContext context, CancellationToken token)
        {
            throw new InvalidOperationException(message.Id.ToString(CultureInfo.InvariantCulture));
        }
    }
}