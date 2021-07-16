namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Internals
{
    using System;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory
    {
        private readonly IStringFormatter _formatter;

        public IntegrationMessageFactory(IStringFormatter formatter)
        {
            _formatter = formatter;
        }

        public IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointIdentity? endpointIdentity, IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage
        {
            var generalMessage = new IntegrationMessage(payload, typeof(TMessage), _formatter);

            if (endpointIdentity != null)
            {
                generalMessage.Headers[IntegrationMessageHeader.SentFrom] = endpointIdentity;
            }

            if (initiatorMessage != null)
            {
                generalMessage.Headers[IntegrationMessageHeader.ConversationId] =
                    initiatorMessage.ReadRequiredHeader<Guid>(IntegrationMessageHeader.ConversationId);
            }

            return generalMessage;
        }
    }
}