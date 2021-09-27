namespace SpaceEngineers.Core.IntegrationTransport.Host.ManualRegistrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Contract;

    internal class TransportEndpointIdentityManualRegistration : IManualRegistration
    {
        private readonly EndpointIdentity _endpointIdentity;

        public TransportEndpointIdentityManualRegistration(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_endpointIdentity);
        }
    }
}