namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using Basics.Attributes;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Messaging.Implementations;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    [Dependent(typeof(IntegrationMessageUserScopeProvider))]
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