namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using IntegrationTransport.InMemory;
    using IntegrationTransport.InMemory.ManualRegistrations;
    using Xunit.Abstractions;

    internal class ModulesTestManualRegistration : IManualRegistration
    {
        private readonly ITestOutputHelper _output;

        public ModulesTestManualRegistration(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            new ModulesTestGenericEndpointIdentityManualRegistration().Register(container);

            new ModulesTestLoggerManualRegistration(_output).Register(container);

            new ManuallyRegisteredServiceManualRegistration().Register(container);

            var endpointInstanceSelectionBehavior = new EndpointInstanceSelectionBehavior();
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(endpointInstanceSelectionBehavior);
            new InMemoryIntegrationTransportManualRegistration(inMemoryIntegrationTransport, endpointInstanceSelectionBehavior).Register(container);

            new MessagesCollectorManualRegistration().Register(container);
            new QueryExpressionsCollectorManualRegistration().Register(container);
        }
    }
}