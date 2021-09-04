namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Implementations
{
    using System;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using MessageHeaders;

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
                generalMessage.WriteHeader(new SentFrom(endpointIdentity));
            }

            if (initiatorMessage != null)
            {
                var conversationId = initiatorMessage.ReadRequiredHeader<ConversationId>().Value;
                generalMessage.WriteHeader(new ConversationId(conversationId));
                generalMessage.WriteHeader(new InitiatorMessageId(initiatorMessage.Id));
            }
            else
            {
                generalMessage.WriteHeader(new ConversationId(Guid.NewGuid()));
            }

            return generalMessage;
        }
    }
}