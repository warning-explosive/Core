namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using GenericHost;

    internal class EndpointIdentityRegistration : IModulesTestRegistration
    {
        public void Register(IRegistrationContainer registration)
        {
            registration.Register<EndpointIdentity>(() => new EndpointIdentity("stub_identity", 0), EnLifestyle.Singleton);
        }
    }
}