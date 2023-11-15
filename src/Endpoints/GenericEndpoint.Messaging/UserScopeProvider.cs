namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class UserScopeProvider : IIntegrationMessageHeaderProvider,
                                       ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            if (initiatorMessage != null && generalMessage.ReadHeader<User>() == null)
            {
                var user = initiatorMessage.ReadHeader<User>()?.Value;

                if (user != null)
                {
                    generalMessage.WriteHeader(new User(user));
                }
            }
        }
    }
}