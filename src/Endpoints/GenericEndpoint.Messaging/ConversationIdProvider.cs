namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class ConversationIdProvider : IIntegrationMessageHeaderProvider,
                                            ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            var conversationId = initiatorMessage != null
                ? initiatorMessage.ReadRequiredHeader<ConversationId>().Value
                : Guid.NewGuid();

            generalMessage.WriteHeader(new ConversationId(conversationId));
        }
    }
}