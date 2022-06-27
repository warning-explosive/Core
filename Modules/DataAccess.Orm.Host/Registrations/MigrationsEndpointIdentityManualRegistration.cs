namespace SpaceEngineers.Core.DataAccess.Orm.Host.Registrations
{
    using GenericEndpoint.Contract;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class MigrationsEndpointIdentityManualRegistration : IManualRegistration
    {
        private readonly EndpointIdentity _endpointIdentity;

        public MigrationsEndpointIdentityManualRegistration(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_endpointIdentity);
        }
    }
}