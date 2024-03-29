namespace SpaceEngineers.Core.GenericEndpoint.Authorization
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class AuthorizationHeaderProvider : IIntegrationMessageHeaderProvider,
                                                 ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            if (initiatorMessage != null && generalMessage.ReadHeader<Authorization>() == null)
            {
                var authorizationToken = initiatorMessage.ReadHeader<Authorization>()?.Value;

                if (authorizationToken != null)
                {
                    generalMessage.WriteHeader(new Authorization(authorizationToken));
                }
            }
        }
    }
}