namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(UserScopeProvider))]
    internal class AnonymousUserScopeProvider : IIntegrationMessageHeaderProvider,
                                                ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        private const string Anonymous = nameof(Anonymous);

        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            if (generalMessage.ReadHeader<User>() == null)
            {
                generalMessage.WriteHeader(new User(Anonymous));
            }
        }
    }
}