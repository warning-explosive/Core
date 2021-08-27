namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class IdentifiedCommandThrowingMessageHandler : MessageHandlerBase<IdentifiedCommand>
    {
        public override Task Handle(IdentifiedCommand message, IIntegrationContext context, CancellationToken token)
        {
            throw new InvalidOperationException(message.Id.ToString(CultureInfo.InvariantCulture));
        }
    }
}