namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;

    internal class MessageHandlerManualRegistration : IManualRegistration
    {
        private readonly Type _messageHandlerType;

        public MessageHandlerManualRegistration(Type messageHandlerType)
        {
            _messageHandlerType = messageHandlerType;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Register(typeof(IMessageHandler<>), _messageHandlerType);
        }
    }
}