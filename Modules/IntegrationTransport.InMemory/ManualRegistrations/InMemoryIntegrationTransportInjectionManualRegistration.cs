namespace SpaceEngineers.Core.IntegrationTransport.InMemory.ManualRegistrations
{
    using Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;

    internal class InMemoryIntegrationTransportInjectionManualRegistration : IManualRegistration
    {
        private readonly IDependencyContainer _dependencyContainer;

        public InMemoryIntegrationTransportInjectionManualRegistration(
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_dependencyContainer.Resolve<IIntegrationTransport>());
            container.RegisterInstance(_dependencyContainer.Resolve<InMemoryIntegrationTransport>());

            container.RegisterInstance(_dependencyContainer.Resolve<IEndpointInstanceSelectionBehavior>());
            container.RegisterInstance(_dependencyContainer.Resolve<EndpointInstanceSelectionBehavior>());
        }
    }
}