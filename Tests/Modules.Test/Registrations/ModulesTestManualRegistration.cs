namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using InMemoryIntegrationTransport.Host.ManualRegistrations;
    using Mocks;

    internal class ModulesTestManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            new ModulesTestGenericEndpointIdentityManualRegistration().Register(container);
            new InMemoryIntegrationTransportManualRegistration().Register(container);
            new MessagesCollectorInstanceManualRegistration(new MessagesCollector()).Register(container);
            new ModulesTestLoggerFactoryManualRegistration().Register(container);
            new ManuallyRegisteredServiceManualRegistration().Register(container);
        }
    }
}