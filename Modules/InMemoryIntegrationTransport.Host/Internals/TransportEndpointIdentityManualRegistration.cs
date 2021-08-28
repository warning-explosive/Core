namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using CompositionRoot.Api.Abstractions;
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