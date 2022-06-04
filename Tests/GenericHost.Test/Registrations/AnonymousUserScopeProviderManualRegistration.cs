namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using GenericEndpoint.Messaging.Abstractions;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class AnonymousUserScopeProviderManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterCollectionEntry<IUserScopeProvider, AnonymousUserScopeProvider>(EnLifestyle.Scoped);
        }
    }
}