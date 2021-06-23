namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Collections.Generic;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;

    internal class MessageHandlerManualRegistration : IManualRegistration
    {
        private readonly IEnumerable<Type> _messageHandlerTypes;

        public MessageHandlerManualRegistration(IEnumerable<Type> messageHandlerTypes)
        {
            _messageHandlerTypes = messageHandlerTypes;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            foreach (var messageHandlerType in _messageHandlerTypes)
            {
                container.Register(typeof(IMessageHandler<>), messageHandlerType);
            }
        }
    }
}