namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class MessageOriginProvider : IIntegrationMessageHeaderProvider,
                                           ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        private readonly EndpointIdentity _endpointIdentity;

        public MessageOriginProvider(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            generalMessage.WriteHeader(new SentFrom(_endpointIdentity));
        }
    }
}