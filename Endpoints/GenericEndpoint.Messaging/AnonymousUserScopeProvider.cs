namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(IntegrationMessageUserScopeProvider))]
    internal class AnonymousUserScopeProvider : IUserScopeProvider,
                                                ICollectionResolvable<IUserScopeProvider>
    {
        private const string Anonymous = nameof(Anonymous);

        public bool TryGetUser(IntegrationMessage? initiatorMessage, out string? user)
        {
            user = Anonymous;
            return true;
        }
    }
}