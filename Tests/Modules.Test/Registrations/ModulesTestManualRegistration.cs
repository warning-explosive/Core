namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using IntegrationTransport.InMemory.ManualRegistrations;

    internal class ModulesTestManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            new ModulesTestGenericEndpointIdentityManualRegistration().Register(container);
            new InMemoryIntegrationTransportManualRegistration().Register(container);
            new MessagesCollectorManualRegistration().Register(container);
            new ModulesTestLoggerFactoryManualRegistration().Register(container);
            new ManuallyRegisteredServiceManualRegistration().Register(container);
        }
    }
}