namespace SpaceEngineers.Core.IntegrationTransport.InMemory.ManualRegistrations
{
    using Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;

    internal class InMemoryIntegrationTransportManualRegistration : IManualRegistration
    {
        private readonly InMemoryIntegrationTransport _inMemoryIntegrationTransport;
        private readonly EndpointInstanceSelectionBehavior _endpointInstanceSelectionBehavior;

        public InMemoryIntegrationTransportManualRegistration(
            InMemoryIntegrationTransport inMemoryIntegrationTransport,
            EndpointInstanceSelectionBehavior endpointInstanceSelectionBehavior)
        {
            _inMemoryIntegrationTransport = inMemoryIntegrationTransport;
            _endpointInstanceSelectionBehavior = endpointInstanceSelectionBehavior;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance<IIntegrationTransport>(_inMemoryIntegrationTransport);
            container.RegisterInstance<IEndpointInstanceSelectionBehavior>(_endpointInstanceSelectionBehavior);
        }
    }
}