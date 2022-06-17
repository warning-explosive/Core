namespace SpaceEngineers.Core.Modules.Test.MessageHandlers
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericEndpoint.Api.Abstractions;
    using Messages;

    [Component(EnLifestyle.Transient)]
    internal class CommandThrowingMessageHandler : IMessageHandler<Command>,
                                                   IResolvable<IMessageHandler<Command>>
    {
        public Task Handle(Command message, CancellationToken token)
        {
            throw new InvalidOperationException(message.Id.ToString(CultureInfo.InvariantCulture));
        }
    }
}