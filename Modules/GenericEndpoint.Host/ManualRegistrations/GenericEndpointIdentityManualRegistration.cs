namespace SpaceEngineers.Core.GenericEndpoint.Host.ManualRegistrations
{
    using CompositionRoot.Api.Abstractions;
    using Contract;

    internal class GenericEndpointIdentityManualRegistration : IManualRegistration
    {
        private readonly EndpointIdentity _endpointIdentity;

        public GenericEndpointIdentityManualRegistration(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_endpointIdentity);
        }
    }
}