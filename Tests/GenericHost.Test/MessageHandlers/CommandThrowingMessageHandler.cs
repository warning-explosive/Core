namespace SpaceEngineers.Core.GenericHost.Test.MessageHandlers
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Messages;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;

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