namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Contract;

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