namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Implementations
{
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationMessageUserScopeProvider : IUserScopeProvider,
                                                         ICollectionResolvable<IUserScopeProvider>
    {
        public bool TryGetUser(IntegrationMessage? initiatorMessage, out string? user)
        {
            if (initiatorMessage != null)
            {
                user = initiatorMessage.ReadRequiredHeader<User>().Value;
                return true;
            }

            user = default;
            return false;
        }
    }
}