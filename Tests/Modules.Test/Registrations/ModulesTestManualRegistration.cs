namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using IntegrationTransport.InMemory;
    using IntegrationTransport.InMemory.ManualRegistrations;

    internal class ModulesTestManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            new ModulesTestGenericEndpointIdentityManualRegistration().Register(container);

            new ModulesTestLoggerFactoryManualRegistration().Register(container);

            new ManuallyRegisteredServiceManualRegistration().Register(container);

            var endpointInstanceSelectionBehavior = new EndpointInstanceSelectionBehavior();
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(endpointInstanceSelectionBehavior);
            new InMemoryIntegrationTransportManualRegistration(inMemoryIntegrationTransport, endpointInstanceSelectionBehavior).Register(container);

            new MessagesCollectorManualRegistration().Register(container);
        }
    }
}