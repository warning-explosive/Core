namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Messaging.Implementations;

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