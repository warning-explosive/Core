namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class MessageInitiatorProvider : IIntegrationMessageHeaderProvider,
                                              ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            if (initiatorMessage != null)
            {
                generalMessage.WriteHeader(new InitiatorMessageId(initiatorMessage.ReadRequiredHeader<Id>().Value));
            }
        }
    }
}